using FastEndpoints;
using low_cost_flight.Data;

namespace low_cost_flight.Flights.UpdateFlightDeal;

public class UpdateFlightDealEndpoint : Endpoint<UpdateFlightDealRequest, UpdateFlightDealResponse>
{
    private readonly AppDbContext _db;

    public UpdateFlightDealEndpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Put("/api/flight-deals/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Aggiorna un'offerta volo esistente";
            s.Description = "Modifica i dati di un'offerta volo salvata nel database";
        });
    }

    public override async Task HandleAsync(UpdateFlightDealRequest req, CancellationToken ct)
    {
        var deal = await _db.FlightDeals.FindAsync([req.Id], ct);

        if (deal == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        deal.DepartureAirport = req.DepartureAirport.ToUpperInvariant();
        deal.ArrivalAirport = req.ArrivalAirport.ToUpperInvariant();
        deal.NameCity = req.NameCity;
        deal.Country = req.Country;
        deal.Price = req.Price;
        deal.AveragePrice = req.AveragePrice;
        deal.DiscountPercentage = req.DiscountPercentage;
        deal.Currency = string.IsNullOrWhiteSpace(req.Currency) ? "EUR" : req.Currency.ToUpperInvariant();
        deal.DurationInMinutes = req.DurationInMinutes;
        deal.Airline = req.Airline;
        deal.FlightLink = req.FlightLink;
        deal.Description = req.Description;
        deal.Stops = req.Stops;

        await _db.SaveChangesAsync(ct);

        var response = new UpdateFlightDealResponse(
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

        await Send.OkAsync(response, ct);
    }
}
