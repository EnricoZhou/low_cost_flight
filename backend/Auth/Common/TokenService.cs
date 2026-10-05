using FastEndpoints.Security;
using low_cost_flight.Entities;

namespace low_cost_flight.Auth.Common;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public AuthResponse GenerateAuthResponse(User user)
    {
        var signingKey = _config["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey non configurata.");
        var expiryDays = int.TryParse(_config["Jwt:ExpiryDays"], out var days) ? days : 7;
        var expiresAt = DateTime.UtcNow.AddDays(expiryDays);

        var token = JwtBearer.CreateToken(
            o =>
            {
                o.SigningKey = signingKey;
                o.ExpireAt = expiresAt;
                o.User.Roles.Add(user.Role);
                o.User.Claims.Add(("sub", user.Id.ToString()));
                o.User.Claims.Add(("UserId", user.Id.ToString()));
                o.User.Claims.Add(("Email", user.Email));
                o.User.Claims.Add(("FullName", user.FullName));
            });

        return new AuthResponse(
            Token: token,
            ExpiresAt: expiresAt,
            User: new UserDto(
                Id: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                PictureUrl: user.PictureUrl,
                Role: user.Role
            )
        );
    }
}
