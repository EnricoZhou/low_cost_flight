namespace low_cost_flight.Flights.GetAllFlightDeals;

public class GetAllFlightDealsRequest
{
    public string? DepartureAirport { get; set; }
    public string? ArrivalAirport { get; set; }
    public decimal? MaxPrice { get; set; }
}
