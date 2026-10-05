namespace low_cost_flight.Auth.Login;

public record LoginRequest(
    string Email,
    string Password
);
