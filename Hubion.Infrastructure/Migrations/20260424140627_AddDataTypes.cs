using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hubion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    clr_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    postgres_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    validation_pattern = table.Column<string>(type: "text", nullable: true),
                    display_format = table.Column<string>(type: "text", nullable: true),
                    is_aggregatable = table.Column<bool>(type: "boolean", nullable: false),
                    aggregation_functions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_types", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "data_types",
                columns: new[] { "id", "aggregation_functions", "clr_type", "display_format", "is_aggregatable", "postgres_type", "type_name", "validation_pattern" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "[]", "System.String", null, false, "text", "string", null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "[\"sum\",\"min\",\"max\",\"avg\",\"count\"]", "System.Int64", null, true, "bigint", "integer", null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "[\"sum\",\"min\",\"max\",\"avg\",\"count\"]", "System.Decimal", null, true, "numeric(18,6)", "decimal", null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "[\"sum\",\"min\",\"max\",\"avg\",\"count\"]", "System.Decimal", "C2", true, "numeric(10,2)", "currency", null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "[\"count\"]", "System.Boolean", null, true, "boolean", "boolean", null },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "[]", "System.DateOnly", null, false, "date", "date", null },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "[]", "System.DateTimeOffset", null, false, "timestamptz", "datetime", null },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "[]", "System.Text.Json.JsonElement", null, false, "jsonb", "json", null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_data_types_type_name",
                schema: "public",
                table: "data_types",
                column: "type_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_types",
                schema: "public");
        }
    }
}
