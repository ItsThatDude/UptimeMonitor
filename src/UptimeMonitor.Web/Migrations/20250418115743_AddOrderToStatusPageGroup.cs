using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToStatusPageGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Order",
                table: "StatusPageMonitorGroupMonitor",
                newName: "SortOrder");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "StatusPageMonitorGroup",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "StatusPageMonitorGroup");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "StatusPageMonitorGroupMonitor",
                newName: "Order");
        }
    }
}
