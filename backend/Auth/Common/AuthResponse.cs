namespace low_cost_flight.Auth.Common;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string? PictureUrl,
    string Role
);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);
