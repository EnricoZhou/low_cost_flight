namespace low_cost_flight.Entities;

public class FlightDeal
{
    public int Id { get; set; }
    public string DepartureAirport { get; set; } = string.Empty;
    public string ArrivalAirport { get; set; } = string.Empty;
    public string NameCity { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal AveragePrice { get; set; }
    public int DiscountPercentage { get; set; }
    public string Currency { get; set; } = "EUR";
    public int DurationInMinutes { get; set; }
    public string Airline { get; set; } = string.Empty;
    public string FlightLink { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Stops { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
