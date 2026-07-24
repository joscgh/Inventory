using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteUserAndCustomerLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerAccountId",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "NoteLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "NoteLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemUniversalId",
                table: "NoteLines",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Document = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerAccountUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerAccountId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAccountUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAccountUsers_CustomerAccounts_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "CustomerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CreatedByUserId",
                table: "Notes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CustomerAccountId",
                table: "Notes",
                column: "CustomerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteLines_CategoryId",
                table: "NoteLines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteLines_CurrencyId",
                table: "NoteLines",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteLines_ItemUniversalId",
                table: "NoteLines",
                column: "ItemUniversalId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccountUsers_CustomerAccountId",
                table: "CustomerAccountUsers",
                column: "CustomerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccountUsers_Email",
                table: "CustomerAccountUsers",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteLines_Categories_CategoryId",
                table: "NoteLines",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteLines_Currencies_CurrencyId",
                table: "NoteLines",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteLines_Items_ItemUniversalId",
                table: "NoteLines",
                column: "ItemUniversalId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_CustomerAccountUsers_CreatedByUserId",
                table: "Notes",
                column: "CreatedByUserId",
                principalTable: "CustomerAccountUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_CustomerAccounts_CustomerAccountId",
                table: "Notes",
                column: "CustomerAccountId",
                principalTable: "CustomerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteLines_Categories_CategoryId",
                table: "NoteLines");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteLines_Currencies_CurrencyId",
                table: "NoteLines");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteLines_Items_ItemUniversalId",
                table: "NoteLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_CustomerAccountUsers_CreatedByUserId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_CustomerAccounts_CustomerAccountId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "CustomerAccountUsers");

            migrationBuilder.DropTable(
                name: "CustomerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Notes_CreatedByUserId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_CustomerAccountId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_NoteLines_CategoryId",
                table: "NoteLines");

            migrationBuilder.DropIndex(
                name: "IX_NoteLines_CurrencyId",
                table: "NoteLines");

            migrationBuilder.DropIndex(
                name: "IX_NoteLines_ItemUniversalId",
                table: "NoteLines");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CustomerAccountId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "NoteLines");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "NoteLines");

            migrationBuilder.DropColumn(
                name: "ItemUniversalId",
                table: "NoteLines");
        }
    }
}
