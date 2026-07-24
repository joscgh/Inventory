using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumerCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CustomerAccountId",
                table: "Notes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ConsumerCustomerId",
                table: "Notes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceNoteId",
                table: "Notes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsumerCustomers",
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
                    table.PrimaryKey("PK_ConsumerCustomers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ConsumerCustomerId",
                table: "Notes",
                column: "ConsumerCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ReferenceNoteId",
                table: "Notes",
                column: "ReferenceNoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_ConsumerCustomers_ConsumerCustomerId",
                table: "Notes",
                column: "ConsumerCustomerId",
                principalTable: "ConsumerCustomers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Notes_ReferenceNoteId",
                table: "Notes",
                column: "ReferenceNoteId",
                principalTable: "Notes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_ConsumerCustomers_ConsumerCustomerId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Notes_ReferenceNoteId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "ConsumerCustomers");

            migrationBuilder.DropIndex(
                name: "IX_Notes_ConsumerCustomerId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_ReferenceNoteId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ConsumerCustomerId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ReferenceNoteId",
                table: "Notes");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerAccountId",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
