using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubion.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class CreateCallRecordsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "call_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    record_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    overall_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    caller_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    call_start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    call_end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    handle_time_seconds = table.Column<int>(type: "integer", nullable: true, computedColumnSql: "EXTRACT(EPOCH FROM (call_end_at - call_start_at))::integer", stored: true),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    tax_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    payment_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    fulfillment_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    tracking_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_id_external = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    recording_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    addresses = table.Column<string>(type: "jsonb", nullable: true),
                    commitment_events = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    flow_execution_state = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    custom_fields = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    api_response_cache = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    telephony_events = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'[]'::jsonb"),
                    sensitive_data = table.Column<string>(type: "jsonb", nullable: true),
                    sensitive_data_stored_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sensitive_data_wiped_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sensitive_wipe_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "call_interactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    call_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    interaction_number = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    flow_id = table.Column<Guid>(type: "uuid", nullable: true),
                    flow_version = table.Column<int>(type: "integer", nullable: true),
                    disposition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    flow_execution_state = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    commitment_events = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    custom_fields = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_interactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_call_interactions_call_records_call_record_id",
                        column: x => x.call_record_id,
                        principalTable: "call_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_call_interactions_call_record",
                table: "call_interactions",
                column: "call_record_id");

            migrationBuilder.CreateIndex(
                name: "idx_call_records_account",
                table: "call_records",
                columns: new[] { "tenant_id", "account_number" });

            migrationBuilder.CreateIndex(
                name: "idx_call_records_active",
                table: "call_records",
                columns: new[] { "tenant_id", "agent_id" },
                filter: "overall_status = 'active'");

            migrationBuilder.CreateIndex(
                name: "idx_call_records_agent_date",
                table: "call_records",
                columns: new[] { "agent_id", "call_start_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_call_records_caller",
                table: "call_records",
                columns: new[] { "tenant_id", "caller_id" });

            migrationBuilder.CreateIndex(
                name: "idx_call_records_campaign_date",
                table: "call_records",
                columns: new[] { "campaign_id", "call_start_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_call_records_tenant",
                table: "call_records",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "call_interactions");

            migrationBuilder.DropTable(
                name: "call_records");
        }
    }
}
