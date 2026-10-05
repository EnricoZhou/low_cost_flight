namespace low_cost_flight.Flights.GetAllFlightDeals;

public record FlightDealDto(
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

public record GetAllFlightDealsResponse(List<FlightDealDto> Deals);
