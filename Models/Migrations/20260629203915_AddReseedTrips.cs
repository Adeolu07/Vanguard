using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class AddReseedTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 6, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 10, 5, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 10, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 9, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 12, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 10, 6, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 9, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 14, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 10, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 13, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 14, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 16, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 11, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 9, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 9, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 10, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 10, 16, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 11, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 11, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 6, 14, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 7, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 10, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 8, 16, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 9, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 9, 13, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 10, 11, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 10, 17, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 11, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 11, 14, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 6, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 5, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 10, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 12, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 6, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 14, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 13, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 14, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 16, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 11, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 7, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 16, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 29, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 29, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "RailwayTrips",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "DepartureTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 24, 14, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 25, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 10, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 26, 16, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 27, 13, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 11, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 17, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 29, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "PickupTime" },
                values: new object[] { new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 29, 14, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}
