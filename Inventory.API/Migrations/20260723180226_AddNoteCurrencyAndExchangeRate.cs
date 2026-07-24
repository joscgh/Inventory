using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteCurrencyAndExchangeRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Notes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "Notes",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CurrencyId",
                table: "Notes",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Currencies_CurrencyId",
                table: "Notes",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Currencies_CurrencyId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_CurrencyId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Notes");
        }
    }
}
