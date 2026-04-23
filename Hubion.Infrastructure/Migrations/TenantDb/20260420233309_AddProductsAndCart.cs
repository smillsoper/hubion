using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubion.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddProductsAndCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cart",
                table: "call_records",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    searchable = table.Column<bool>(type: "boolean", nullable: false),
                    reporting_only = table.Column<bool>(type: "boolean", nullable: false),
                    parent_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    full_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    allow_price_override = table.Column<bool>(type: "boolean", nullable: false),
                    weight = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    shipping = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    shipping_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    canada_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    akhi_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    outlying_us_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    foreign_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    auto_ship = table.Column<bool>(type: "boolean", nullable: false),
                    auto_ship_optional = table.Column<bool>(type: "boolean", nullable: false),
                    allow_ship_to = table.Column<bool>(type: "boolean", nullable: false),
                    ship_to_required = table.Column<bool>(type: "boolean", nullable: false),
                    allow_delivery_message = table.Column<bool>(type: "boolean", nullable: false),
                    ship_method_per_item = table.Column<bool>(type: "boolean", nullable: false),
                    mix_match_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_upsell = table.Column<bool>(type: "boolean", nullable: false),
                    upsell_qty = table.Column<int>(type: "integer", nullable: false),
                    upsell_qty_of_entry = table.Column<int>(type: "integer", nullable: false),
                    upsell_commission = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    upsell_client_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    inventory_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    decrement_on_order = table.Column<bool>(type: "boolean", nullable: false),
                    qty_available = table.Column<int>(type: "integer", nullable: false),
                    minimum_qty = table.Column<int>(type: "integer", nullable: false),
                    qty_limit = table.Column<int>(type: "integer", nullable: false),
                    qty_limit_exception = table.Column<int>(type: "integer", nullable: false),
                    expected_quantity = table.Column<int>(type: "integer", nullable: false),
                    expected_stock_date = table.Column<DateOnly>(type: "date", nullable: true),
                    backorder_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    discontinued_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    alias_skus = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    keywords = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
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
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_products_parent_product_id",
                        column: x => x.parent_product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_kits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    is_variable = table.Column<bool>(type: "boolean", nullable: false),
                    kit_prompt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    multi_select = table.Column<bool>(type: "boolean", nullable: false),
                    choice_skus = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_kits", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_kits_products_child_product_id",
                        column: x => x.child_product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_kits_products_parent_product_id",
                        column: x => x.parent_product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_kits_child",
                table: "product_kits",
                column: "child_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_kits_parent",
                table: "product_kits",
                column: "parent_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_mix_match_code",
                table: "products",
                column: "mix_match_code");

            migrationBuilder.CreateIndex(
                name: "ix_products_parent_product_id",
                table: "products",
                column: "parent_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_searchable_status",
                table: "products",
                columns: new[] { "searchable", "inventory_status" });

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                table: "products",
                column: "sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_kits");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropColumn(
                name: "cart",
                table: "call_records");
        }
    }
}
