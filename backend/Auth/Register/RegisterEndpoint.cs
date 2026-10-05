using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using low_cost_flight.Auth.Common;
using low_cost_flight.Data;
using low_cost_flight.Entities;

namespace low_cost_flight.Auth.Register;

public class RegisterEndpoint : Endpoint<RegisterRequest, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public RegisterEndpoint(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
        Description(b => b
            .Produces<AuthResponse>(200)
            .ProducesProblem(400)
            .WithTags("Authentication"));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var emailNormalized = req.Email.Trim().ToLowerInvariant();

        var existingUser = await _db.Users
            .AnyAsync(u => u.Email == emailNormalized, ct);

        if (existingUser)
        {
            AddError("Un utente con questo indirizzo email è già registrato.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var newUser = new User
        {
            Email = emailNormalized,
            PasswordHash = passwordHash,
            FullName = req.FullName.Trim(),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync(ct);

        var response = _tokenService.GenerateAuthResponse(newUser);
        await Send.OkAsync(response, ct);
    }
}
