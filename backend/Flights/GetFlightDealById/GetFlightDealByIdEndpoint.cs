using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using low_cost_flight.Data;

namespace low_cost_flight.Flights.GetFlightDealById;

public class GetFlightDealByIdEndpoint : Endpoint<GetFlightDealByIdRequest, GetFlightDealByIdResponse>
{
    private readonly AppDbContext _db;

    public GetFlightDealByIdEndpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/flight-deals/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Recupera una singola offerta volo tramite ID";
            s.Description = "Cerca un'offerta volo salvata nel database per chiave primaria ID";
        });
    }

    public override async Task HandleAsync(GetFlightDealByIdRequest req, CancellationToken ct)
    {
        var deal = await _db.FlightDeals
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == req.Id, ct);

        if (deal == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new GetFlightDealByIdResponse(
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
