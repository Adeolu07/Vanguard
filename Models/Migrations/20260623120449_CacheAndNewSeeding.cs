using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class CacheAndNewSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletTokens");

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Price", "TotalSeats" },
                values: new object[] { 45, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 8, 0, 0, 0, DateTimeKind.Unspecified), 4500m, 45 });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "Price", "TotalSeats" },
                values: new object[] { 50, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 7, 0, 0, 0, DateTimeKind.Unspecified), "Benin", 8500m, 50 });

            migrationBuilder.InsertData(
                table: "BusTrips",
                columns: new[] { "Id", "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "IsActive", "Price", "TotalSeats" },
                values: new object[,]
                {
                    { 3, 48, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 6, 0, 0, 0, DateTimeKind.Unspecified), "Abuja", "Lagos", true, 12000m, 48 },
                    { 4, 50, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 9, 0, 0, 0, DateTimeKind.Unspecified), "Port Harcourt", "Lagos", true, 10500m, 50 },
                    { 5, 40, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 5, 0, 0, 0, DateTimeKind.Unspecified), "Kano", "Lagos", true, 15000m, 40 },
                    { 6, 45, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 10, 0, 0, 0, DateTimeKind.Unspecified), "Kaduna", "Abuja", true, 3500m, 45 },
                    { 7, 42, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 7, 0, 0, 0, DateTimeKind.Unspecified), "Jos", "Abuja", true, 4200m, 42 },
                    { 8, 50, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 8, 0, 0, 0, DateTimeKind.Unspecified), "Enugu", "Abuja", true, 7500m, 50 },
                    { 9, 30, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 12, 0, 0, 0, DateTimeKind.Unspecified), "Aba", "Port Harcourt", true, 2800m, 30 },
                    { 10, 40, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 6, 0, 0, 0, DateTimeKind.Unspecified), "Calabar", "Port Harcourt", true, 5500m, 40 },
                    { 11, 35, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 9, 0, 0, 0, DateTimeKind.Unspecified), "Asaba", "Benin", true, 3200m, 35 },
                    { 12, 36, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 7, 0, 0, 0, DateTimeKind.Unspecified), "Katsina", "Kano", true, 3800m, 36 },
                    { 13, 45, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 14, 0, 0, 0, DateTimeKind.Unspecified), "Lagos", "Ibadan", true, 4500m, 45 },
                    { 14, 40, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 8, 0, 0, 0, DateTimeKind.Unspecified), "Owerri", "Ibadan", true, 6800m, 40 },
                    { 15, 42, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 13, 0, 0, 0, DateTimeKind.Unspecified), "Abuja", "Jos", true, 4200m, 42 }
                });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 280, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 8, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", 3000m, "Lagos–Ibadan", 280, "Standard" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 280, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 15, 0, 0, 0, DateTimeKind.Unspecified), "Lagos", "Ibadan", 3000m, "Lagos–Ibadan", 280, "Standard" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 100, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 7, 0, 0, 0, DateTimeKind.Unspecified), 9000m, "Abuja–Kaduna", 100, "First" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 100, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 14, 0, 0, 0, DateTimeKind.Unspecified), 9000m, "Abuja–Kaduna", 100, "First" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 240, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 9, 0, 0, 0, DateTimeKind.Unspecified), "Itakpe", "Warri", 2500m, "Warri–Itakpe", 240, "Standard" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 240, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 16, 0, 0, 0, DateTimeKind.Unspecified), "Warri", "Itakpe", 2500m, "Warri–Itakpe", 240, "Standard" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 80, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 7, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", 5800m, "Lagos–Ibadan", 80, "Business" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 95, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 11, 0, 0, 0, DateTimeKind.Unspecified), "Kaduna", "Abuja", 6500m, "Abuja–Kaduna", 95, "Business" });

            migrationBuilder.InsertData(
                table: "RailwayTrips",
                columns: new[] { "Id", "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "IsActive", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[,]
                {
                    { 9, 160, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 8, 0, 0, 0, DateTimeKind.Unspecified), "Aba", "Port Harcourt", true, 2000m, "Port Harcourt–Aba", 160, "Standard" },
                    { 10, 160, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 15, 0, 0, 0, DateTimeKind.Unspecified), "Port Harcourt", "Aba", true, 2000m, "Port Harcourt–Aba", 160, "Standard" },
                    { 11, 180, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 7, 0, 0, 0, DateTimeKind.Unspecified), "Abeokuta", "Lagos", true, 1500m, "Lagos–Abeokuta", 180, "Standard" },
                    { 12, 180, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 16, 0, 0, 0, DateTimeKind.Unspecified), "Lagos", "Abeokuta", true, 1500m, "Lagos–Abeokuta", 180, "Standard" },
                    { 13, 200, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 29, 8, 0, 0, 0, DateTimeKind.Unspecified), "Minna", "Abuja", true, 2800m, "Abuja–Minna", 200, "Standard" },
                    { 14, 200, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 29, 15, 0, 0, 0, DateTimeKind.Unspecified), "Abuja", "Minna", true, 2800m, "Abuja–Minna", 200, "Standard" },
                    { 15, 48, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 9, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", true, 7500m, "Lagos–Ibadan", 48, "First" }
                });

            migrationBuilder.InsertData(
                table: "TaxiTrips",
                columns: new[] { "Id", "CreatedAt", "DropoffLocation", "IsActive", "MaxPassengers", "PickupLocation", "PickupTime", "Price", "VehicleType" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Victoria Island", true, 4, "Ikeja", new DateTime(2026, 6, 24, 9, 0, 0, 0, DateTimeKind.Unspecified), 12000m, null },
                    { 2, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lekki", true, 4, "Ikeja", new DateTime(2026, 6, 24, 14, 0, 0, 0, DateTimeKind.Unspecified), 10000m, null },
                    { 3, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ikeja", true, 4, "Lekki", new DateTime(2026, 6, 25, 8, 0, 0, 0, DateTimeKind.Unspecified), 10000m, null },
                    { 4, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ikeja", true, 4, "Victoria Island", new DateTime(2026, 6, 25, 15, 0, 0, 0, DateTimeKind.Unspecified), 12000m, null },
                    { 5, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Garki", true, 4, "Abuja CBD", new DateTime(2026, 6, 26, 10, 0, 0, 0, DateTimeKind.Unspecified), 4500m, null },
                    { 6, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Wuse", true, 4, "Garki", new DateTime(2026, 6, 26, 16, 0, 0, 0, DateTimeKind.Unspecified), 3500m, null },
                    { 7, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Maitama", true, 4, "Wuse", new DateTime(2026, 6, 27, 8, 0, 0, 0, DateTimeKind.Unspecified), 4000m, null },
                    { 8, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rumuokoro", true, 4, "Port Harcourt", new DateTime(2026, 6, 27, 13, 0, 0, 0, DateTimeKind.Unspecified), 5000m, null },
                    { 9, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sabon Gari", true, 4, "Kano", new DateTime(2026, 6, 28, 11, 0, 0, 0, DateTimeKind.Unspecified), 3000m, null },
                    { 10, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mokola", true, 4, "Ibadan", new DateTime(2026, 6, 28, 17, 0, 0, 0, DateTimeKind.Unspecified), 3500m, null },
                    { 11, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ugbowo", true, 4, "Benin", new DateTime(2026, 6, 29, 8, 0, 0, 0, DateTimeKind.Unspecified), 4000m, null },
                    { 12, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Haven", true, 4, "Enugu", new DateTime(2026, 6, 29, 14, 0, 0, 0, DateTimeKind.Unspecified), 3800m, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.CreateTable(
                name: "WalletTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTokens", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Price", "TotalSeats" },
                values: new object[] { 40, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), 5000m, 40 });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "Price", "TotalSeats" },
                values: new object[] { 40, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 14, 8, 0, 0, 0, DateTimeKind.Unspecified), "Jigawa", 5000m, 40 });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 100, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), "Kaduna", "Abuja", 9000m, "AKTS", 100, "First" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 150, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), "Kaduna", "Abuja", 6500m, "AKTS", 150, "Business" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 200, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Unspecified), 3600m, "AKTS", 200, "Regular" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 200, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 20, 14, 0, 0, 0, DateTimeKind.Unspecified), 3600m, "AKTS", 200, "Regular" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 300, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", 3000m, "Lagos-Ibadan", 300, "Regular" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 300, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 21, 15, 0, 0, 0, DateTimeKind.Unspecified), "Lagos", "Ibadan", 3000m, "Lagos-Ibadan", 300, "Regular" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 250, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 22, 9, 0, 0, 0, DateTimeKind.Unspecified), "Itakpe", "Warri", 2500m, "Warri-Itakpe", 250, "Regular" });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "Price", "Route", "TotalSeats", "TrainClass" },
                values: new object[] { 250, new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), "Warri", "Itakpe", 2500m, "Warri-Itakpe", 250, "Regular" });
        }
    }
}
