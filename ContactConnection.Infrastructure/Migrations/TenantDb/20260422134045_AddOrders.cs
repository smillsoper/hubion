using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactConnection.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    call_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    shipping = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    sales_tax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ship_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    payment_breakdowns = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    shipped_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    extended_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    shipping = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    sales_tax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    weight = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    shipping_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    tax_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    on_back_order = table.Column<bool>(type: "boolean", nullable: false),
                    auto_ship = table.Column<bool>(type: "boolean", nullable: false),
                    auto_ship_interval_days = table.Column<int>(type: "integer", nullable: false),
                    is_upsell = table.Column<bool>(type: "boolean", nullable: false),
                    mix_match_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ship_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    delivery_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ship_to = table.Column<string>(type: "jsonb", nullable: true),
                    canada_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    akhi_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    outlying_us_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    foreign_surcharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    payments = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    personalization_answers = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    kit_selections = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    fulfillment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tracking_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shipped_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_lines_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_lines_order_id",
                table: "order_lines",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_lines_tenant_id",
                table: "order_lines",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_call_record_id",
                table: "orders",
                column: "call_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id",
                table: "orders",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_status",
                table: "orders",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_lines");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
