using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class ReSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1,
                column: "DepartureTime",
                value: new DateTime(2026, 6, 12, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2,
                column: "DepartureTime",
                value: new DateTime(2026, 6, 14, 8, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 1,
                column: "DepartureTime",
                value: new DateTime(2026, 6, 8, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "BusTrips",
                keyColumn: "Id",
                keyValue: 2,
                column: "DepartureTime",
                value: new DateTime(2026, 6, 10, 8, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
