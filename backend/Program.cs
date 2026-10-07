using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Audit.Core;
using Audit.EntityFramework;
using low_cost_flight.Auth.Common;
using low_cost_flight.Data;
using low_cost_flight.Entities;

var builder = WebApplication.CreateBuilder(args);

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configurazione Audit.NET: salva le modifiche nella tabella AuditLogs
Audit.Core.Configuration.Setup()
    .UseEntityFramework(ef => ef
        .UseDbContext<AppDbContext>()
        .AuditTypeExplicitMapper(m => m
            .Map<FlightDeal, AuditLog>((ev, entry, audit) =>
            {
                audit.TableName = entry.Table;
                audit.Action = entry.Action;
                audit.EventDate = DateTime.UtcNow;
                audit.UserName = ev.Environment?.UserName;
                audit.AuditData = entry.ToJson();
            })
            .Map<User, AuditLog>((ev, entry, audit) =>
            {
                audit.TableName = entry.Table;
                audit.Action = entry.Action;
                audit.EventDate = DateTime.UtcNow;
                audit.UserName = ev.Environment?.UserName;
                audit.AuditData = entry.ToJson();
            })
        )
        .IgnoreMatchedProperties(true)
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "LowCostFlight_";
});

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] 
    ??  throw new InvalidOperationException("La configurazione 'Jwt:SigningKey' è obbligatoria e non è stata trovata.");

builder.Services.AddAuthenticationJwtBearer(s => s.SigningKey = jwtSigningKey);
builder.Services.AddAuthorization();

builder.Services.AddSingleton<TokenService>();
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "SkyDealRadar API";
        s.Version = "v1";
    };
    o.EnableJWTBearerAuth = true;
});
builder.Services.AddHttpClient();

var app = builder.Build();

// CORS middleware
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();
