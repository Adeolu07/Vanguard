using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class AvailableSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "TaxiTrips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "TaxiTrips",
                keyColumn: "Id",
                keyValue: 1,
                column: "AvailableSeats",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "TaxiTrips");
        }
    }
}
