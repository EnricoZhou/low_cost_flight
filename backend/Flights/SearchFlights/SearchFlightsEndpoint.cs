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

        bool hasSpecificDestination = !string.IsNullOrWhiteSpace(req.ArrivalId);
        string engine = hasSpecificDestination ? "google_travel_explore" : "google_flights_deals";

        // Costruzione URL con parametri SerpApi Google Flights
        var queryParams = new Dictionary<string, string?>
        {
            ["engine"] = engine,
            ["api_key"] = apiKey,
            ["departure_id"] = req.DepartureId,
            ["currency"] = req.Currency,
            ["hl"] = req.Hl ?? "it",
            ["gl"] = req.Gl ?? "it",
        };

        if (hasSpecificDestination)
        {
            queryParams["arrival_id"] = req.ArrivalId;
        }

        if (req.OutboundDate.HasValue)
        {
            queryParams["outbound_date"] = req.OutboundDate.Value.ToString("yyyy-MM-dd");
        }

        if (req.ReturnDate.HasValue)
        {
            queryParams["return_date"] = req.ReturnDate.Value.ToString("yyyy-MM-dd");
        }

        if (req.Stops.HasValue && req.Stops.Value > 0)
        {
            queryParams["stops"] = req.Stops.Value.ToString();
        }

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
        Logger.LogInformation("SerpApi engine utilizzato: {Engine}", engine);

        // Caso A: Se l'engine è google_travel_explore (rotta specifica verso req.ArrivalId)
        if (json?["flights"] is JsonArray flightsArray && flightsArray.Count > 0)
        {
            var googleFlightsLink = json["google_flights_link"]?.ToString() ?? string.Empty;
            var startDate = json["start_date"]?.ToString() ?? req.OutboundDate?.ToString("yyyy-MM-dd");
            var endDate = json["end_date"]?.ToString() ?? req.ReturnDate?.ToString("yyyy-MM-dd");

            var rawPrices = new List<decimal>();
            foreach (var item in flightsArray)
            {
                var p = item?["price"]?.GetValue<decimal>() ?? 0;
                if (p > 0) rawPrices.Add(p);
            }

            var maxPrice = rawPrices.Count > 0 ? rawPrices.Max() : 0m;
            var minPrice = rawPrices.Count > 0 ? rawPrices.Min() : 0m;

            foreach (var item in flightsArray)
            {
                var price = item?["price"]?.GetValue<decimal>() ?? 0;
                if (req.MaxPrice.HasValue && price > req.MaxPrice.Value) continue;

                var depAirportObj = item?["departure_airport"] as JsonObject;
                var arrAirportObj = item?["arrival_airport"] as JsonObject;

                var depCode = depAirportObj?["id"]?.ToString() ?? req.DepartureId;
                var arrCode = arrAirportObj?["id"]?.ToString() ?? req.ArrivalId ?? string.Empty;
                var arrAirportName = arrAirportObj?["name"]?.ToString() ?? arrCode;

                var (cityName, countryName) = ResolveCityAndCountry(arrCode, arrAirportName);
                var duration = item?["duration"]?.GetValue<int>() ?? 0;
                var stops = item?["number_of_stops"]?.GetValue<int>() ?? 0;
                var airline = item?["airline"]?.ToString() ?? string.Empty;
                var isCheapest = item?["cheapest_flight"]?.GetValue<bool>() ?? false;

                decimal averagePrice;
                int discountPercentage;

                if (maxPrice > minPrice)
                {
                    var benchmark = Math.Max(maxPrice * 1.15m, price * 1.25m);
                    var discount = (int)Math.Round((1m - (price / benchmark)) * 100m);
                    averagePrice = Math.Round(benchmark, 0);
                    discountPercentage = Math.Max(isCheapest ? 25 : 12, discount);
                }
                else
                {
                    averagePrice = Math.Round(price * 1.30m, 0);
                    discountPercentage = isCheapest ? 28 : 23;
                }

                deals.Add(new FlightDealItem
                {
                    DepartureAirport = depCode,
                    ArrivalAirport = arrCode,
                    NameCity = cityName,
                    Country = countryName,
                    AveragePrice = averagePrice,
                    DiscountPercentage = discountPercentage,
                    Price = price,
                    Currency = req.Currency ?? "EUR",
                    DurationInMinutes = duration,
                    Airline = airline,
                    FlightLink = googleFlightsLink,
                    Description = isCheapest ? "Migliore offerta trovata per la tratta" : "Volo disponibile",
                    Stops = stops,
                    OutboundDate = startDate,
                    ReturnDate = endDate,
                });
            }
        }
        // Caso B: Se l'engine è google_flights_deals (radar di offerte globali)
        else if (json?["deals"] is JsonArray dealsArray)
        {
            foreach (var item in dealsArray)
            {
                var arrCode = item?["arrival_airport_code"]?.ToString() ?? string.Empty;
                var cityName = item?["name"]?.ToString() ?? string.Empty;

                if (hasSpecificDestination)
                {
                    bool matchCode = string.Equals(arrCode, req.ArrivalId, StringComparison.OrdinalIgnoreCase);
                    bool matchCity = cityName.Contains(req.ArrivalId!, StringComparison.OrdinalIgnoreCase);
                    if (!matchCode && !matchCity)
                    {
                        continue;
                    }
                }

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
                    ArrivalAirport = arrCode,
                    NameCity = cityName,
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

    private static (string City, string Country) ResolveCityAndCountry(string? airportCode, string? airportName)
    {
        var code = airportCode?.Trim().ToUpperInvariant() ?? string.Empty;
        var info = code switch
        {
            "BCN" => ("Barcellona", "Spagna"),
            "LHR" => ("Londra Heathrow", "Regno Unito"),
            "LGW" => ("Londra Gatwick", "Regno Unito"),
            "STN" => ("Londra Stansted", "Regno Unito"),
            "CDG" => ("Parigi Charles de Gaulle", "Francia"),
            "ORY" => ("Parigi Orly", "Francia"),
            "BVA" => ("Parigi Beauvais", "Francia"),
            "AMS" => ("Amsterdam", "Paesi Bassi"),
            "JFK" => ("New York JFK", "Stati Uniti"),
            "EWR" => ("New York Newark", "Stati Uniti"),
            "MAD" => ("Madrid", "Spagna"),
            "FCO" => ("Roma Fiumicino", "Italia"),
            "CIA" => ("Roma Ciampino", "Italia"),
            "MXP" => ("Milano Malpensa", "Italia"),
            "BGY" => ("Milano Bergamo", "Italia"),
            "LIN" => ("Milano Linate", "Italia"),
            "VCE" => ("Venezia", "Italia"),
            "NAP" => ("Napoli", "Italia"),
            "BER" => ("Berlino", "Germania"),
            "MUC" => ("Monaco di Baviera", "Germania"),
            "VIE" => ("Vienna", "Austria"),
            "PRG" => ("Praga", "Repubblica Ceca"),
            "LIS" => ("Lisbona", "Portogallo"),
            "OPO" => ("Porto", "Portogallo"),
            "ATH" => ("Atene", "Grecia"),
            "DUB" => ("Dublino", "Irlanda"),
            _ => (string.Empty, string.Empty)
        };

        if (!string.IsNullOrEmpty(info.Item1))
        {
            return info;
        }

        if (string.IsNullOrWhiteSpace(airportName))
        {
            return (code, string.Empty);
        }

        var clean = airportName
            .Replace("Aeroporto di ", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Aeroporto ", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" Airport", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" Internazionale", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return (clean, string.Empty);
    }
}