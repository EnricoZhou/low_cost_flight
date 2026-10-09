namespace low_cost_flight.Flights.SearchFlights;

public record SearchFlightsResponse(IReadOnlyList<FlightDealItem> Deals);


public record FlightDealItem
{
    public string DepartureAirport { get; init; } = string.Empty;
    public string ArrivalAirport { get; init; } = string.Empty;
    public string NameCity { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public decimal AveragePrice { get; init; }
    public int DiscountPercentage { get; init; } = 0;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "EUR";
    public int DurationInMinutes { get; init; }
    public string Airline { get; init; } = string.Empty;
    public string FlightLink { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stops { get; init; }
    public string? OutboundDate { get; init; }
    public string? ReturnDate { get; init; }    
}