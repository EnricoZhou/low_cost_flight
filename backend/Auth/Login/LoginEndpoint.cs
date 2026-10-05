using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using low_cost_flight.Auth.Common;
using low_cost_flight.Data;

namespace low_cost_flight.Auth.Login;

public class LoginEndpoint : Endpoint<LoginRequest, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public LoginEndpoint(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Description(b => b
            .Produces<AuthResponse>(200)
            .ProducesProblem(400)
            .WithTags("Authentication"));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var emailNormalized = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == emailNormalized, ct);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            AddError("Credenziali non valide. Verifica l'email e la password inserite.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            AddError("Credenziali non valide. Verifica l'email e la password inserite.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var response = _tokenService.GenerateAuthResponse(user);
        await Send.OkAsync(response, ct);
    }
}
