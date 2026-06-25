using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class AddRailwayTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RailwayTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TrainClass = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RailwayTrips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RailwayBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RailwayTripId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RailwayBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RailwayBookings_RailwayTrips_RailwayTripId",
                        column: x => x.RailwayTripId,
                        principalTable: "RailwayTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RailwayBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RailwayTrips",
                columns: new[] { "Id", "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "IsActive", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[,]
                {
                    { 1, 100, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), "Kaduna", "Abuja", true, 9000m, "AKTS", 100, "First" },
                    { 2, 150, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), "Kaduna", "Abuja", true, 6500m, "AKTS", 150, "Business" },
                    { 3, 200, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), "Kaduna", "Abuja", true, 3600m, "AKTS", 200, "Regular" },
                    { 4, 200, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 14, 0, 0, 0, DateTimeKind.Unspecified), "Abuja", "Kaduna", true, 3600m, "AKTS", 200, "Regular" },
                    { 5, 300, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", true, 3000m, "Lagos-Ibadan", 300, "Regular" },
                    { 6, 300, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 21, 15, 0, 0, 0, DateTimeKind.Unspecified), "Lagos", "Ibadan", true, 3000m, "Lagos-Ibadan", 300, "Regular" },
                    { 7, 250, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 22, 9, 0, 0, 0, DateTimeKind.Unspecified), "Itakpe", "Warri", true, 2500m, "Warri-Itakpe", 250, "Regular" },
                    { 8, 250, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), "Warri", "Itakpe", true, 2500m, "Warri-Itakpe", 250, "Regular" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RailwayBookings_RailwayTripId",
                table: "RailwayBookings",
                column: "RailwayTripId");

            migrationBuilder.CreateIndex(
                name: "IX_RailwayBookings_UserId",
                table: "RailwayBookings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RailwayBookings");

            migrationBuilder.DropTable(
                name: "RailwayTrips");
        }
    }
}
