using Microsoft.EntityFrameworkCore;
using Audit.EntityFramework;
using low_cost_flight.Entities;

namespace low_cost_flight.Data;

public class AppDbContext : AuditDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<FlightDeal> FlightDeals => Set<FlightDeal>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<FlightDeal>(entity =>
        {
            // Precisione per i prezzi decimali (10 cifre totali, 2 decimali)
            entity.Property(e => e.Price)
                  .HasPrecision(10, 2);
            entity.Property(e => e.AveragePrice)
                  .HasPrecision(10, 2);
            // Lunghezze per codici IATA/ICAO e valuta
            entity.Property(e => e.DepartureAirport)
                  .HasMaxLength(3);
            entity.Property(e => e.ArrivalAirport)
                  .HasMaxLength(3);
            entity.Property(e => e.Currency)
                  .HasMaxLength(3);
            entity.Property(e => e.Airline)
                  .HasMaxLength(100);
            entity.Property(e => e.FlightLink)
                  .HasMaxLength(2048);
            // Indici per velocizzare ricerche frequenti
            entity.HasIndex(e => new { e.DepartureAirport, e.ArrivalAirport });
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.TableName)
                  .HasMaxLength(100);
            entity.Property(e => e.Action)
                  .HasMaxLength(50);
            entity.Property(e => e.UserName)
                  .HasMaxLength(100);
            entity.HasIndex(e => e.EventDate);
        });
    }
}

