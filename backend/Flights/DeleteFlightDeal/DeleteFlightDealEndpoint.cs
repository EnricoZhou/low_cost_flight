using FastEndpoints;
using low_cost_flight.Data;

namespace low_cost_flight.Flights.DeleteFlightDeal;

public class DeleteFlightDealEndpoint : Endpoint<DeleteFlightDealRequest>
{
    private readonly AppDbContext _db;

    public DeleteFlightDealEndpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Delete("/api/flight-deals/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Elimina un'offerta volo dal database";
            s.Description = "Rimuove definitivamente un'offerta volo dalla tabella FlightDeals per ID";
        });
    }

    public override async Task HandleAsync(DeleteFlightDealRequest req, CancellationToken ct)
    {
        var deal = await _db.FlightDeals.FindAsync([req.Id], ct);

        if (deal == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        _db.FlightDeals.Remove(deal);
        await _db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
