using FastEndpoints;
using FluentValidation;

namespace low_cost_flight.Flights.SearchFlights
{
    public class SearchFlightsValidator : Validator<SearchFlightsRequest>
    {
        public SearchFlightsValidator()
        {
            RuleFor(x => x.DepartureId).NotEmpty().WithMessage("Departure airport is required").Length(3).WithMessage("Departure airport must be 3 characters long (e.g. FCO)");
            RuleFor(x => x.ArrivalId).Length(3).WithMessage("Arrival airport must be 3 characters long (e.g. CDG)");
            RuleFor(x => x.MaxPrice).GreaterThan(0).WithMessage("Max price must be greater than 0");
        }
    }
}