using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "Notes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Notes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfitMargin",
                table: "Items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Adjustments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerAccountId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Rif = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountLocations_CustomerAccounts_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "CustomerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountLogos",
                columns: table => new
                {
                    CustomerAccountId = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLogos", x => x.CustomerAccountId);
                    table.ForeignKey(
                        name: "FK_AccountLogos_CustomerAccounts_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "CustomerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemStocks",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemStocks", x => new { x.ItemId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_ItemStocks_AccountLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "AccountLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemStocks_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Terminals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerAccountId = table.Column<int>(type: "integer", nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Terminals_AccountLocations_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AccountLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terminals_CustomerAccounts_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "CustomerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceNumberRanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TerminalId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: false),
                    FromNumber = table.Column<long>(type: "bigint", nullable: false),
                    ToNumber = table.Column<long>(type: "bigint", nullable: false),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false),
                    ControlPrefix = table.Column<string>(type: "text", nullable: false),
                    ControlFromNumber = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Authorization = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceNumberRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceNumberRanges_Terminals_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    ControlNumber = table.Column<string>(type: "text", nullable: false),
                    TerminalId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceNumberRangeId = table.Column<int>(type: "integer", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerAccountId = table.Column<int>(type: "integer", nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: true),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ConsumerCustomerId = table.Column<int>(type: "integer", nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    CustomerDocument = table.Column<string>(type: "text", nullable: false),
                    CustomerAddress = table.Column<string>(type: "text", nullable: false),
                    CustomerPhone = table.Column<string>(type: "text", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    ReferenceInvoiceId = table.Column<int>(type: "integer", nullable: true),
                    VoidedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VoidReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_AccountLocations_StoreId",
                        column: x => x.StoreId,
                        principalTable: "AccountLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_AccountLocations_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AccountLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_ConsumerCustomers_ConsumerCustomerId",
                        column: x => x.ConsumerCustomerId,
                        principalTable: "ConsumerCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_CustomerAccountUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "CustomerAccountUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_CustomerAccounts_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "CustomerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_InvoiceNumberRanges_InvoiceNumberRangeId",
                        column: x => x.InvoiceNumberRangeId,
                        principalTable: "InvoiceNumberRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Invoices_ReferenceInvoiceId",
                        column: x => x.ReferenceInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Terminals_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ItemUniversalId = table.Column<int>(type: "integer", nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    CurrencyId = table.Column<int>(type: "integer", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Items_ItemUniversalId",
                        column: x => x.ItemUniversalId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_StoreId",
                table: "Notes",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_WarehouseId",
                table: "Notes",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Adjustments_LocationId",
                table: "Adjustments",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountLocations_CustomerAccountId_Name",
                table: "AccountLocations",
                columns: new[] { "CustomerAccountId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_CategoryId",
                table: "InvoiceLines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_CurrencyId",
                table: "InvoiceLines",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ItemUniversalId",
                table: "InvoiceLines",
                column: "ItemUniversalId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberRanges_TerminalId_DocumentType_FromNumber",
                table: "InvoiceNumberRanges",
                columns: new[] { "TerminalId", "DocumentType", "FromNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientGuid",
                table: "Invoices",
                column: "ClientGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ConsumerCustomerId",
                table: "Invoices",
                column: "ConsumerCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedByUserId",
                table: "Invoices",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CurrencyId",
                table: "Invoices",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerAccountId_DocumentType_Serie_Number",
                table: "Invoices",
                columns: new[] { "CustomerAccountId", "DocumentType", "Serie", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumberRangeId",
                table: "Invoices",
                column: "InvoiceNumberRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ReferenceInvoiceId",
                table: "Invoices",
                column: "ReferenceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StoreId",
                table: "Invoices",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TerminalId",
                table: "Invoices",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_WarehouseId",
                table: "Invoices",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemStocks_LocationId",
                table: "ItemStocks",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminals_CustomerAccountId_Code",
                table: "Terminals",
                columns: new[] { "CustomerAccountId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terminals_CustomerAccountId_Serie",
                table: "Terminals",
                columns: new[] { "CustomerAccountId", "Serie" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terminals_StoreId",
                table: "Terminals",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adjustments_AccountLocations_LocationId",
                table: "Adjustments",
                column: "LocationId",
                principalTable: "AccountLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_AccountLocations_StoreId",
                table: "Notes",
                column: "StoreId",
                principalTable: "AccountLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_AccountLocations_WarehouseId",
                table: "Notes",
                column: "WarehouseId",
                principalTable: "AccountLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adjustments_AccountLocations_LocationId",
                table: "Adjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_AccountLocations_StoreId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_AccountLocations_WarehouseId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "AccountLogos");

            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "ItemStocks");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "InvoiceNumberRanges");

            migrationBuilder.DropTable(
                name: "Terminals");

            migrationBuilder.DropTable(
                name: "AccountLocations");

            migrationBuilder.DropIndex(
                name: "IX_Notes_StoreId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_WarehouseId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Adjustments_LocationId",
                table: "Adjustments");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ProfitMargin",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Adjustments");
        }
    }
}
