using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    EmailConfirmationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmationTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailConfirmationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserWalletId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LicenseId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VehicleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MarshalId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusTrips_Users_MarshalId",
                        column: x => x.MarshalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RailwayTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TrainClass = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MarshalId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RailwayTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RailwayTrips_Users_MarshalId",
                        column: x => x.MarshalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxiTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickupLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DropoffLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxPassengers = table.Column<int>(type: "int", nullable: false),
                    PickupTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MarshalId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxiTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxiTrips_Users_MarshalId",
                        column: x => x.MarshalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BusTripId = table.Column<int>(type: "int", nullable: true),
                    TaxiTripId = table.Column<int>(type: "int", nullable: true),
                    RailwayTripId = table.Column<int>(type: "int", nullable: true),
                    TransportType = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    NumberOfSeats = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentTraceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_BusTrips_BusTripId",
                        column: x => x.BusTripId,
                        principalTable: "BusTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_RailwayTrips_RailwayTripId",
                        column: x => x.RailwayTripId,
                        principalTable: "RailwayTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_TaxiTrips_TaxiTripId",
                        column: x => x.TaxiTripId,
                        principalTable: "TaxiTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    PassengerId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransportType = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    TripTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fare = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidatedByMarshalId = table.Column<int>(type: "int", nullable: true),
                    QrCodeBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                columns: new[] { "Id", "CreatedAt", "DropoffLocation", "IsActive", "MarshalId", "MaxPassengers", "PickupLocation", "PickupTime", "Price", "VehicleId", "VehicleType" },
                values: new object[] { 1, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Victoria Island", true, 103, 4, "Ikeja", new DateTime(2026, 7, 6, 9, 0, 0, 0, DateTimeKind.Unspecified), 12000m, "VEH-TAXI-P5Q6R7S8", null });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BusTripId",
                table: "Bookings",
                column: "BusTripId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RailwayTripId",
                table: "Bookings",
                column: "RailwayTripId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TaxiTripId",
                table: "Bookings",
                column: "TaxiTripId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusTrips_MarshalId",
                table: "BusTrips",
                column: "MarshalId");

            migrationBuilder.CreateIndex(
                name: "IX_RailwayTrips_MarshalId",
                table: "RailwayTrips",
                column: "MarshalId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxiTrips_MarshalId",
                table: "TaxiTrips",
                column: "MarshalId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookingId",
                table: "Tickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketReference",
                table: "Tickets",
                column: "TicketReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthTokens");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "BusTrips");

            migrationBuilder.DropTable(
                name: "RailwayTrips");

            migrationBuilder.DropTable(
                name: "TaxiTrips");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
