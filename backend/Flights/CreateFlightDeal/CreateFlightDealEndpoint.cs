using FastEndpoints;
using low_cost_flight.Data;
using low_cost_flight.Entities;

namespace low_cost_flight.Flights.CreateFlightDeal;

public class CreateFlightDealEndpoint : Endpoint<CreateFlightDealRequest, CreateFlightDealResponse>
{
    private readonly AppDbContext _db;

    public CreateFlightDealEndpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/flight-deals");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Crea una nuova offerta volo nel database";
            s.Description = "Inserisce una nuova offerta volo con tutti i dettagli nella tabella FlightDeals";
        });
    }

    public override async Task HandleAsync(CreateFlightDealRequest req, CancellationToken ct)
    {
        var deal = new FlightDeal
        {
            DepartureAirport = req.DepartureAirport.ToUpperInvariant(),
            ArrivalAirport = req.ArrivalAirport.ToUpperInvariant(),
            NameCity = req.NameCity,
            Country = req.Country,
            Price = req.Price,
            AveragePrice = req.AveragePrice,
            DiscountPercentage = req.DiscountPercentage,
            Currency = string.IsNullOrWhiteSpace(req.Currency) ? "EUR" : req.Currency.ToUpperInvariant(),
            DurationInMinutes = req.DurationInMinutes,
            Airline = req.Airline,
            FlightLink = req.FlightLink,
            Description = req.Description,
            Stops = req.Stops,
            CreatedAt = DateTime.UtcNow
        };

        _db.FlightDeals.Add(deal);
        await _db.SaveChangesAsync(ct);

        var response = new CreateFlightDealResponse(
            deal.Id,
            deal.DepartureAirport,
            deal.ArrivalAirport,
            deal.NameCity,
            deal.Country,
            deal.Price,
            deal.AveragePrice,
            deal.DiscountPercentage,
            deal.Currency,
            deal.DurationInMinutes,
            deal.Airline,
            deal.FlightLink,
            deal.Description,
            deal.Stops,
            deal.CreatedAt
        );

        await Send.CreatedAtAsync($"/api/flight-deals/{deal.Id}", response, response, cancellation: ct);
    }
}
