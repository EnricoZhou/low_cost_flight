using FastEndpoints;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using low_cost_flight.Auth.Common;
using low_cost_flight.Data;
using low_cost_flight.Entities;

namespace low_cost_flight.Auth.GoogleLogin;

public class GoogleLoginEndpoint : Endpoint<GoogleLoginRequest, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleLoginEndpoint> _logger;

    public GoogleLoginEndpoint(
        AppDbContext db,
        TokenService tokenService,
        IConfiguration config,
        ILogger<GoogleLoginEndpoint> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/auth/google");
        AllowAnonymous();
        Description(b => b
            .Produces<AuthResponse>(200)
            .ProducesProblem(400)
            .WithTags("Authentication"));
    }

    public override async Task HandleAsync(GoogleLoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.IdToken))
        {
            AddError("Il token Google (IdToken) è obbligatorio.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        string email;
        string name;
        string? picture = null;
        string googleSub;

        // Supporto per test / demo locale rapido se il token è 'demo-google-token'
        if (req.IdToken.StartsWith("demo-google-token", StringComparison.OrdinalIgnoreCase))
        {
            email = "demo.google.user@example.com";
            name = "Google Demo Explorer";
            picture = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=120&q=80";
            googleSub = "google_demo_sub_12345";
        }
        else
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings();
                var configuredClientId = _config["Google:ClientId"];

                if (!string.IsNullOrEmpty(configuredClientId) &&
                    !configuredClientId.Contains("your-google-client-id", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Audience = new[] { configuredClientId };
                }

                var payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);
                email = payload.Email;
                name = payload.Name ?? payload.Email;
                picture = payload.Picture;
                googleSub = payload.Subject;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validazione Google ID Token fallita.");
                AddError("Validazione del token Google fallita. Assicurati che il token sia valido e non scaduto.");
                await Send.ErrorsAsync(400, ct);
                return;
            }
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Cerca utente esistente per email o googleId
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail || u.GoogleId == googleSub, ct);

        if (user == null)
        {
            // Crea nuovo utente autenticato tramite Google
            user = new User
            {
                Email = normalizedEmail,
                FullName = name,
                PictureUrl = picture,
                GoogleId = googleSub,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
        }
        else
        {
            // Aggiorna profilo Google
            user.GoogleId ??= googleSub;
            if (!string.IsNullOrEmpty(picture)) user.PictureUrl = picture;
            if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(user.FullName)) user.FullName = name;
            user.LastLoginAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        var response = _tokenService.GenerateAuthResponse(user);
        await Send.OkAsync(response, ct);
    }
}
