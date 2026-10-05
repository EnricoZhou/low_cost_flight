using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace low_cost_flight.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightDeals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartureAirport = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ArrivalAirport = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    NameCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AveragePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DurationInMinutes = table.Column<int>(type: "int", nullable: false),
                    Airline = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FlightLink = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stops = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightDeals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightDeals_CreatedAt",
                table: "FlightDeals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FlightDeals_DepartureAirport_ArrivalAirport",
                table: "FlightDeals",
                columns: new[] { "DepartureAirport", "ArrivalAirport" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightDeals");
        }
    }
}
