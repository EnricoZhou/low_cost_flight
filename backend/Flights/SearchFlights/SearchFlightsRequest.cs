namespace low_cost_flight.Flights.SearchFlights;

public record SearchFlightsRequest
{
    public string DepartureId { get; init; } = string.Empty;
    public string? ArrivalId { get; init; }
    public string? Currency { get; init; } = "EUR";
    public DateOnly? OutboundDate { get; init; }
    public DateOnly? ReturnDate { get; init; }
    public int? MaxPrice { get; init; }
    public string? Hl { get; init; } = "it";
    public string? Gl {get; init;} = "it";

    // 0 - Any number of stops (default)
    // 1 - Nonstop only
    // 2 - 1 stop or fewer
    // 3 - 2 stops or fewer
    public int? Stops {get; init;} = 0; 

}