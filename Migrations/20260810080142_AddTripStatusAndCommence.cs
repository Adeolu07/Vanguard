using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class AddTripStatusAndCommence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaxiTrips");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RailwayTrips");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BusTrips");

            migrationBuilder.AddColumn<DateTime>(
                name: "CommencedAt",
                table: "TaxiTrips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TaxiTrips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommencedAt",
                table: "RailwayTrips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RailwayTrips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommencedAt",
                table: "BusTrips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BusTrips",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommencedAt",
                table: "TaxiTrips");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TaxiTrips");

            migrationBuilder.DropColumn(
                name: "CommencedAt",
                table: "RailwayTrips");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RailwayTrips");

            migrationBuilder.DropColumn(
                name: "CommencedAt",
                table: "BusTrips");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BusTrips");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaxiTrips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RailwayTrips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BusTrips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ConfirmationTokenExpiry", "CreatedAt", "Email", "EmailConfirmationSentAt", "EmailConfirmationToken", "FirstName", "IsActive", "IsEmailConfirmed", "LastName", "LicenseId", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiry", "PhoneNumber", "Role", "UserWalletId", "VehicleId", "VehicleType" },
                values: new object[,]
                {
                    { 101, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "busmarshal@tripfinity.com", null, null, "Bus", true, true, "Marshal", "BUS-LIC-001", "Welcome123", null, null, "08010000001", "Marshal", "WALLET-BUS-M001", "VEH-BUS-A1B2C3D4", "Bus" },
                    { 102, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "railwaymarshal@tripfinity.com", null, null, "Railway", true, true, "Marshal", "RAIL-LIC-001", "Welcome123", null, null, "08010000002", "Marshal", "WALLET-RAIL-M001", "VEH-RAIL-X9Y8Z7W6", "Railway" },
                    { 103, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "taximarshal@tripfinity.com", null, null, "Taxi", true, true, "Marshal", "TAXI-LIC-001", "Welcome123", null, null, "08010000003", "Marshal", "WALLET-TAXI-M001", "VEH-TAXI-P5Q6R7S8", "Taxi" }
                });

            migrationBuilder.InsertData(
                table: "BusTrips",
                columns: new[] { "Id", "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "IsActive", "MarshalId", "Price", "TotalSeats", "VehicleId" },
                values: new object[,]
                {
                    { 1, 45, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 8, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", true, 101, 4500m, 45, "VEH-BUS-A1B2C3D4" },
                    { 2, 50, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 7, 0, 0, 0, DateTimeKind.Unspecified), "Benin", "Lagos", true, 101, 8500m, 50, "VEH-BUS-A1B2C3D4" }
                });

            migrationBuilder.InsertData(
                table: "RailwayTrips",
                columns: new[] { "Id", "AvailableSeats", "CreatedAt", "DepartureTime", "Destination", "From", "IsActive", "MarshalId", "Price", "Route", "TotalSeats", "TrainClass", "VehicleId" },
                values: new object[] { 1, 280, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 8, 0, 0, 0, DateTimeKind.Unspecified), "Ibadan", "Lagos", true, 102, 3000m, "Lagos–Ibadan", 280, "Standard", "VEH-RAIL-X9Y8Z7W6" });

            migrationBuilder.InsertData(
                table: "TaxiTrips",
                columns: new[] { "Id", "AvailableSeats", "CreatedAt", "DropoffLocation", "IsActive", "MarshalId", "MaxPassengers", "PickupLocation", "PickupTime", "Price", "VehicleId", "VehicleType" },
                values: new object[] { 1, 0, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Victoria Island", true, 103, 4, "Ikeja", new DateTime(2026, 7, 6, 9, 0, 0, 0, DateTimeKind.Unspecified), 12000m, "VEH-TAXI-P5Q6R7S8", null });
        }
    }
}
