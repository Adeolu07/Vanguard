using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Tripfinity.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQrTokenFromTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_QrToken",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "QrCodeBase64",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketReference",
                table: "Tickets",
                column: "TicketReference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_TicketReference",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "QrCodeBase64",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "QrToken",
                table: "Tickets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_QrToken",
                table: "Tickets",
                column: "QrToken",
                unique: true);
        }
    }
}
