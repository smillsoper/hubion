using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubion.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddCustomFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "custom_field_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_type_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    validation_rules = table.Column<string>(type: "jsonb", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_field_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custom_field_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    call_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value_string = table.Column<string>(type: "text", nullable: true),
                    value_integer = table.Column<long>(type: "bigint", nullable: true),
                    value_decimal = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    value_boolean = table.Column<bool>(type: "boolean", nullable: true),
                    value_date = table.Column<DateOnly>(type: "date", nullable: true),
                    value_datetime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    value_json = table.Column<string>(type: "jsonb", nullable: true),
                    stored_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_field_values", x => x.id);
                    table.ForeignKey(
                        name: "FK_custom_field_values_custom_field_definitions_definition_id",
                        column: x => x.definition_id,
                        principalTable: "custom_field_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cfd_scope",
                table: "custom_field_definitions",
                columns: new[] { "tenant_id", "client_id", "campaign_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cfd_tenant_field_scope_unique",
                table: "custom_field_definitions",
                columns: new[] { "tenant_id", "field_name", "client_id", "campaign_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cfd_tenant_id",
                table: "custom_field_definitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_cfv_call_record_definition_unique",
                table: "custom_field_values",
                columns: new[] { "call_record_id", "definition_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cfv_call_record_id",
                table: "custom_field_values",
                column: "call_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_custom_field_values_definition_id",
                table: "custom_field_values",
                column: "definition_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "custom_field_values");

            migrationBuilder.DropTable(
                name: "custom_field_definitions");
        }
    }
}
