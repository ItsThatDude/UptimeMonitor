using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToMonitorResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonitorResults_Timestamp",
                table: "MonitorResults");

            migrationBuilder.CreateIndex(
                name: "IX_MonitorResults_Timestamp_MonitorConfigurationId_WorkerConfi~",
                table: "MonitorResults",
                columns: new[] { "Timestamp", "MonitorConfigurationId", "WorkerConfigurationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonitorResults_Timestamp_MonitorConfigurationId_WorkerConfi~",
                table: "MonitorResults");

            migrationBuilder.CreateIndex(
                name: "IX_MonitorResults_Timestamp",
                table: "MonitorResults",
                column: "Timestamp");
        }
    }
}
