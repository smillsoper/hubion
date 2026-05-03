using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactConnection.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateTenantsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subdomain = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    custom_domain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: true),
                    schema_name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    plan_tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    trial_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    billing_contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    feature_flags = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_custom_domain",
                schema: "public",
                table: "tenants",
                column: "custom_domain",
                unique: true,
                filter: "custom_domain IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_schema_name",
                schema: "public",
                table: "tenants",
                column: "schema_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_subdomain",
                schema: "public",
                table: "tenants",
                column: "subdomain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenants",
                schema: "public");
        }
    }
}
