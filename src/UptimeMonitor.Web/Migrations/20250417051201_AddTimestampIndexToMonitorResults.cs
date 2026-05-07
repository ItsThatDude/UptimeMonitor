using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampIndexToMonitorResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MonitorResults_Timestamp",
                table: "MonitorResults",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonitorResults_Timestamp",
                table: "MonitorResults");
        }
    }
}
