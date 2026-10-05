using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Audit.Core;
using Audit.EntityFramework;
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
        )
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "LowCostFlight_";
});


builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.AddHttpClient();

var app = builder.Build();

// CORS middleware
app.UseCors("AllowAngular");

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();
