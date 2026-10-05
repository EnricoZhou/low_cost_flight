namespace low_cost_flight.Flights.CreateFlightDeal;

public record CreateFlightDealResponse(
    int Id,
    string DepartureAirport,
    string ArrivalAirport,
    string NameCity,
    string Country,
    decimal Price,
    decimal AveragePrice,
    int DiscountPercentage,
    string Currency,
    int DurationInMinutes,
    string Airline,
    string FlightLink,
    string Description,
    int Stops,
    DateTime CreatedAt
);
