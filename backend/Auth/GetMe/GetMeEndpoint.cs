using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using low_cost_flight.Auth.Common;
using low_cost_flight.Data;

namespace low_cost_flight.Auth.GetMe;

public class GetMeEndpoint : EndpointWithoutRequest<UserDto>
{
    private readonly AppDbContext _db;

    public GetMeEndpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/auth/me");
        Description(b => b
            .Produces<UserDto>(200)
            .Produces(401)
            .Produces(404)
            .WithTags("Authentication"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var userDto = new UserDto(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            PictureUrl: user.PictureUrl,
            Role: user.Role
        );

        await Send.OkAsync(userDto, ct);
    }
}
