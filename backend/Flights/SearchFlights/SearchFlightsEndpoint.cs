using System.Text.Json;
using System.Text.Json.Nodes;
using FastEndpoints;
using Microsoft.Extensions.Caching.Distributed;

namespace low_cost_flight.Flights.SearchFlights;

public class SearchFlightsEndpoint : Endpoint<SearchFlightsRequest, SearchFlightsResponse>
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IDistributedCache _cache;

    public SearchFlightsEndpoint(HttpClient httpClient, IConfiguration config, IDistributedCache cache)
    {
        _httpClient = httpClient;
        _config = config;
        _cache = cache;
    }

    public override void Configure()
    {
        Get("/api/flights/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchFlightsRequest req, CancellationToken ct)
    {
        // 1. Creiamo una chiave univoca basata sui parametri di ricerca
        var cacheKey = $"flight_deals:{req.DepartureId}:{req.ArrivalId}:{req.OutboundDate}:{req.ReturnDate}:{req.Currency}:{req.Stops}:{req.MaxPrice}".ToLower();

        // 2. Controllo Cache: se presente in Redis, restituiamo subito i dati senza chiamare SerpApi
        var cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedDeals = JsonSerializer.Deserialize<List<FlightDealItem>>(cachedJson);
            if (cachedDeals != null)
            {
                Logger.LogInformation("Dati recuperati da Redis Cache per chiave: {CacheKey}", cacheKey);
                await Send.OkAsync(new SearchFlightsResponse(cachedDeals), ct);
                return;
            }
        }
        var apiKey = _config["SerpApi:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ThrowError("Chiave API SerpApi non configurata in appsettings.json (SerpApi:ApiKey)", 500);
        }

        // Costruzione URL con parametri SerpApi Google Flights
        var queryParams = new Dictionary<string, string?>
        {
            ["engine"] = "google_flights_deals",
            ["api_key"] = apiKey,
            ["departure_id"] = req.DepartureId,
            ["arrival_id"] = req.ArrivalId,
            ["outbound_date"] = req.OutboundDate?.ToString("yyyy-MM-dd"),
            ["return_date"] = req.ReturnDate?.ToString("yyyy-MM-dd"),
            ["currency"] = req.Currency,
            ["hl"] = req.Hl,
            ["gl"] = req.Gl,
            ["stops"] = req.Stops?.ToString(),
        };

        // Se l'utente specifica solo la data di andata, impostiamo type=2 (One-Way / Solo andata)
        // altrimenti SerpApi richiede obbligatoriamente return_date per i voli round-trip (type=1).
        if (req.OutboundDate.HasValue && !req.ReturnDate.HasValue)
        {
            queryParams["type"] = "2";
        }
        else if (req.OutboundDate.HasValue && req.ReturnDate.HasValue)
        {
            queryParams["type"] = "1";
        }

        var query = string.Join("&", queryParams
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));

        var response = await _httpClient.GetAsync($"https://serpapi.com/search.json?{query}", ct);
        if (!response.IsSuccessStatusCode)
        {
            ThrowError("Error call APi SerpApi", (int)response.StatusCode);
        }

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct);
        var deals = new List<FlightDealItem>();
        Logger.LogInformation($"response json: {json}");

        // 1. Se l'engine è google_flights_deals, SerpApi restituisce l'array "deals"
        JsonArray? dealsArray = json?["deals"] as JsonArray;
        if (dealsArray != null)
        {
            foreach (var item in dealsArray)
            {
                var price = item?["price"]?.GetValue<decimal>() ?? 0;
                if (req.MaxPrice.HasValue && price > req.MaxPrice.Value) continue;

                var discountPercentage = item?["discount_percentage"]?.GetValue<int>() ?? 0;
                var averagePrice = item?["average_price"]?.GetValue<decimal>() ?? 0;

                // Se SerpApi non ha esplicitato la percentuale ma il prezzo medio storico è superiore, calcoliamola
                if (discountPercentage <= 0 && averagePrice > price && averagePrice > 0)
                {
                    discountPercentage = (int)Math.Round((1m - (price / averagePrice)) * 100m);
                }

                // Includiamo SOLO ed esclusivamente i voli con sconto reale
                if (discountPercentage <= 0)
                {
                    continue;
                }

                deals.Add(new FlightDealItem
                {
                    DepartureAirport = item?["departure_airport_code"]?.ToString() ?? req.DepartureId,
                    ArrivalAirport = item?["arrival_airport_code"]?.ToString() ?? req.ArrivalId ?? string.Empty,
                    NameCity = item?["name"]?.ToString() ?? string.Empty,
                    Country = item?["country"]?.ToString() ?? string.Empty,
                    AveragePrice = averagePrice,
                    DiscountPercentage = discountPercentage,
                    Price = price,
                    Currency = item?["currency"]?.ToString() ?? string.Empty,
                    DurationInMinutes = item?["flight_duration"]?.GetValue<int>() ?? 0,
                    Airline = item?["airline"]?.ToString() ?? string.Empty,
                    FlightLink = item?["flight_link"]?.ToString() ?? string.Empty,
                    Description = item?["description"]?.ToString() ?? string.Empty,
                    Stops = item?["stops"]?.GetValue<int>() ?? 0,
                    OutboundDate = item?["outbound_date"]?.ToString(),
                    ReturnDate = item?["return_date"]?.ToString(),
                });
            }
        }
        else
        {
            var errorMsg = json?["error"]?.ToString() ?? "Nessuna offerta trovata per la ricerca specificata.";
            ThrowError(errorMsg, 404);
        }

        // 3. Salviamo i risultati in Redis con una scadenza (TTL) di 10 minuti
        if (deals.Count > 0)
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(deals),
                cacheOptions,
                ct
            );
            Logger.LogInformation("Risultati memorizzati in cache per 10 minuti con chiave: {CacheKey}", cacheKey);
        }

        await Send.OkAsync(new SearchFlightsResponse(deals), ct);
    }
}