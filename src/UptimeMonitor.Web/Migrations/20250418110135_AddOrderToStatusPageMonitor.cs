using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToStatusPageMonitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatusPageMonitorGroupMonitors");

            migrationBuilder.CreateTable(
                name: "StatusPageMonitorGroupMonitor",
                columns: table => new
                {
                    MonitorId = table.Column<int>(type: "integer", nullable: false),
                    MonitorGroupId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPageMonitorGroupMonitor", x => new { x.MonitorGroupId, x.MonitorId });
                    table.ForeignKey(
                        name: "FK_StatusPageMonitorGroupMonitor_MonitorConfigurations_Monitor~",
                        column: x => x.MonitorId,
                        principalTable: "MonitorConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatusPageMonitorGroupMonitor_StatusPageMonitorGroup_Monito~",
                        column: x => x.MonitorGroupId,
                        principalTable: "StatusPageMonitorGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageMonitorGroupMonitor_MonitorId",
                table: "StatusPageMonitorGroupMonitor",
                column: "MonitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatusPageMonitorGroupMonitor");

            migrationBuilder.CreateTable(
                name: "StatusPageMonitorGroupMonitors",
                columns: table => new
                {
                    MonitorsId = table.Column<int>(type: "integer", nullable: false),
                    StatusPageMonitorGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPageMonitorGroupMonitors", x => new { x.MonitorsId, x.StatusPageMonitorGroupId });
                    table.ForeignKey(
                        name: "FK_StatusPageMonitorGroupMonitors_MonitorConfigurations_Monito~",
                        column: x => x.MonitorsId,
                        principalTable: "MonitorConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatusPageMonitorGroupMonitors_StatusPageMonitorGroup_Statu~",
                        column: x => x.StatusPageMonitorGroupId,
                        principalTable: "StatusPageMonitorGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageMonitorGroupMonitors_StatusPageMonitorGroupId",
                table: "StatusPageMonitorGroupMonitors",
                column: "StatusPageMonitorGroupId");
        }
    }
}
