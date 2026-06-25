using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class BookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "RailwayBookings");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Bookings",
                newName: "TotalAmount");

            migrationBuilder.AlterColumn<int>(
                name: "BusTripId",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfSeats",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RailwayTripId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxiTripId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportType",
                table: "Bookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TaxiTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickupLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DropoffLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxPassengers = table.Column<int>(type: "int", nullable: false),
                    PickupTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxiTrips", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RailwayTripId",
                table: "Bookings",
                column: "RailwayTripId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TaxiTripId",
                table: "Bookings",
                column: "TaxiTripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_RailwayTrips_RailwayTripId",
                table: "Bookings",
                column: "RailwayTripId",
                principalTable: "RailwayTrips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_TaxiTrips_TaxiTripId",
                table: "Bookings",
                column: "TaxiTripId",
                principalTable: "TaxiTrips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_RailwayTrips_RailwayTripId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TaxiTrips_TaxiTripId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "TaxiTrips");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_RailwayTripId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TaxiTripId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "NumberOfSeats",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RailwayTripId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TaxiTripId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransportType",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Bookings",
                newName: "Amount");

            migrationBuilder.AlterColumn<int>(
                name: "BusTripId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RailwayBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RailwayTripId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_RailwayBookings_RailwayTripId",
                table: "RailwayBookings",
                column: "RailwayTripId");

            migrationBuilder.CreateIndex(
                name: "IX_RailwayBookings_UserId",
                table: "RailwayBookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
