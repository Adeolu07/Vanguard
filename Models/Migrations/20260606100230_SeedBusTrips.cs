using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class SeedBusTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BusTrips",
                columns: new[] { "Id", "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "IsActive", "Price", "TotalSeats" },
                values: new object[,]
                {
                    { 1, 40, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 8, 8, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", true, 5000m, 40 },
                    { 2, 40, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), "Jigawa", "Lagos", true, 5000m, 40 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
