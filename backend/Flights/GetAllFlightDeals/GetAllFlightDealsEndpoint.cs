using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using low_cost_flight.Data;

namespace low_cost_flight.Flights.GetAllFlightDeals;

public class GetAllFlightDealsEndpoint : Endpoint<GetAllFlightDealsRequest, GetAllFlightDealsResponse>
{
    private readonly AppDbContext _db;

    public GetAllFlightDealsEndpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/flight-deals");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Recupera tutte le offerte volo dal database";
            s.Description = "Restituisce l'elenco delle offerte salvate, con filtri opzionali per aeroporto e prezzo massimo";
        });
    }

    public override async Task HandleAsync(GetAllFlightDealsRequest req, CancellationToken ct)
    {
        var query = _db.FlightDeals.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.DepartureAirport))
        {
            var dep = req.DepartureAirport.Trim().ToUpperInvariant();
            query = query.Where(d => d.DepartureAirport == dep);
        }

        if (!string.IsNullOrWhiteSpace(req.ArrivalAirport))
        {
            var arr = req.ArrivalAirport.Trim().ToUpperInvariant();
            query = query.Where(d => d.ArrivalAirport == arr);
        }

        if (req.MaxPrice.HasValue)
        {
            query = query.Where(d => d.Price <= req.MaxPrice.Value);
        }

        var deals = await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new FlightDealDto(
                d.Id,
                d.DepartureAirport,
                d.ArrivalAirport,
                d.NameCity,
                d.Country,
                d.Price,
                d.AveragePrice,
                d.DiscountPercentage,
                d.Currency,
                d.DurationInMinutes,
                d.Airline,
                d.FlightLink,
                d.Description,
                d.Stops,
                d.CreatedAt
            ))
            .ToListAsync(ct);

        await Send.OkAsync(new GetAllFlightDealsResponse(deals), ct);
    }
}
