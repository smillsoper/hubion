using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactConnection.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddFlowsAndFlowSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flow_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flow_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flow_version = table.Column<int>(type: "integer", nullable: false),
                    call_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    interaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_node_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    variable_store = table.Column<string>(type: "jsonb", nullable: false),
                    execution_history = table.Column<string>(type: "jsonb", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flow_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flows",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    flow_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    definition = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_agent_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flows", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_flow_sessions_active_call",
                table: "flow_sessions",
                column: "call_record_id",
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "idx_flow_sessions_agent_date",
                table: "flow_sessions",
                columns: new[] { "tenant_id", "agent_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "idx_flow_sessions_call_record_status",
                table: "flow_sessions",
                columns: new[] { "call_record_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_flows_tenant_active",
                table: "flows",
                columns: new[] { "tenant_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "idx_flows_tenant_client_campaign",
                table: "flows",
                columns: new[] { "tenant_id", "client_id", "campaign_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flow_sessions");

            migrationBuilder.DropTable(
                name: "flows");
        }
    }
}
