using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubion.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddInventoryReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "qty_reserved",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "qty_reserved",
                table: "products");
        }
    }
}
