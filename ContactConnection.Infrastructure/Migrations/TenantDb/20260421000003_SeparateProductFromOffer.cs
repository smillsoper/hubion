using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactConnection.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class SeparateProductFromOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_mix_match_code",
                table: "products");

            migrationBuilder.DropColumn(
                name: "allow_delivery_message",
                table: "products");

            migrationBuilder.DropColumn(
                name: "allow_price_override",
                table: "products");

            migrationBuilder.DropColumn(
                name: "allow_ship_to",
                table: "products");

            migrationBuilder.DropColumn(
                name: "auto_ship",
                table: "products");

            migrationBuilder.DropColumn(
                name: "auto_ship_intervals",
                table: "products");

            migrationBuilder.DropColumn(
                name: "auto_ship_optional",
                table: "products");

            migrationBuilder.DropColumn(
                name: "flags",
                table: "products");

            migrationBuilder.DropColumn(
                name: "full_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "is_upsell",
                table: "products");

            migrationBuilder.DropColumn(
                name: "mix_match_code",
                table: "products");

            migrationBuilder.DropColumn(
                name: "mix_match_price_breaks",
                table: "products");

            migrationBuilder.DropColumn(
                name: "payments",
                table: "products");

            migrationBuilder.DropColumn(
                name: "personalization",
                table: "products");

            migrationBuilder.DropColumn(
                name: "quantity_price_breaks",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ship_method_per_item",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ship_methods",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ship_to_required",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_exempt",
                table: "products");

            migrationBuilder.DropColumn(
                name: "tax_exempt",
                table: "products");

            migrationBuilder.DropColumn(
                name: "upsell_client_amount",
                table: "products");

            migrationBuilder.DropColumn(
                name: "upsell_commission",
                table: "products");

            migrationBuilder.DropColumn(
                name: "upsell_qty",
                table: "products");

            migrationBuilder.DropColumn(
                name: "upsell_qty_of_entry",
                table: "products");

            migrationBuilder.CreateTable(
                name: "offers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    full_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    allow_price_override = table.Column<bool>(type: "boolean", nullable: false),
                    shipping = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    shipping_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    mix_match_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_upsell = table.Column<bool>(type: "boolean", nullable: false),
                    upsell_qty = table.Column<int>(type: "integer", nullable: false),
                    upsell_qty_of_entry = table.Column<int>(type: "integer", nullable: false),
                    upsell_commission = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    upsell_client_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    auto_ship = table.Column<bool>(type: "boolean", nullable: false),
                    auto_ship_optional = table.Column<bool>(type: "boolean", nullable: false),
                    allow_ship_to = table.Column<bool>(type: "boolean", nullable: false),
                    ship_to_required = table.Column<bool>(type: "boolean", nullable: false),
                    allow_delivery_message = table.Column<bool>(type: "boolean", nullable: false),
                    ship_method_per_item = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    payments = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    quantity_price_breaks = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    mix_match_price_breaks = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    auto_ship_intervals = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    ship_methods = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    personalization = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    flags = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offers", x => x.id);
                    table.ForeignKey(
                        name: "FK_offers_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_offers_mix_match_code",
                table: "offers",
                column: "mix_match_code");

            migrationBuilder.CreateIndex(
                name: "ix_offers_product_id",
                table: "offers",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_tenant_active",
                table: "offers",
                columns: new[] { "tenant_id", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "offers");

            migrationBuilder.AddColumn<bool>(
                name: "allow_delivery_message",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "allow_price_override",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "allow_ship_to",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "auto_ship",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "auto_ship_intervals",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<bool>(
                name: "auto_ship_optional",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "flags",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<decimal>(
                name: "full_price",
                table: "products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_upsell",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "mix_match_code",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mix_match_price_breaks",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "payments",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "personalization",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "quantity_price_breaks",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<bool>(
                name: "ship_method_per_item",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ship_methods",
                table: "products",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<bool>(
                name: "ship_to_required",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping",
                table: "products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "shipping_exempt",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "tax_exempt",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "upsell_client_amount",
                table: "products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "upsell_commission",
                table: "products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "upsell_qty",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "upsell_qty_of_entry",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_products_mix_match_code",
                table: "products",
                column: "mix_match_code");
        }
    }
}
