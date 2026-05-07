using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class StatusPagesFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitorStatusPageMonitorGroup");

            migrationBuilder.DropTable(
                name: "Monitor");

            migrationBuilder.CreateTable(
                name: "MonitorConfigurationStatusPageMonitorGroup",
                columns: table => new
                {
                    MonitorsId = table.Column<int>(type: "integer", nullable: false),
                    StatusPageMonitorGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorConfigurationStatusPageMonitorGroup", x => new { x.MonitorsId, x.StatusPageMonitorGroupId });
                    table.ForeignKey(
                        name: "FK_MonitorConfigurationStatusPageMonitorGroup_MonitorConfigura~",
                        column: x => x.MonitorsId,
                        principalTable: "MonitorConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonitorConfigurationStatusPageMonitorGroup_StatusPageMonito~",
                        column: x => x.StatusPageMonitorGroupId,
                        principalTable: "StatusPageMonitorGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitorConfigurationStatusPageMonitorGroup_StatusPageMonito~",
                table: "MonitorConfigurationStatusPageMonitorGroup",
                column: "StatusPageMonitorGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitorConfigurationStatusPageMonitorGroup");

            migrationBuilder.CreateTable(
                name: "Monitor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitorType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monitor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitorStatusPageMonitorGroup",
                columns: table => new
                {
                    MonitorsId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusPageMonitorGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorStatusPageMonitorGroup", x => new { x.MonitorsId, x.StatusPageMonitorGroupId });
                    table.ForeignKey(
                        name: "FK_MonitorStatusPageMonitorGroup_Monitor_MonitorsId",
                        column: x => x.MonitorsId,
                        principalTable: "Monitor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonitorStatusPageMonitorGroup_StatusPageMonitorGroup_Status~",
                        column: x => x.StatusPageMonitorGroupId,
                        principalTable: "StatusPageMonitorGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitorStatusPageMonitorGroup_StatusPageMonitorGroupId",
                table: "MonitorStatusPageMonitorGroup",
                column: "StatusPageMonitorGroupId");
        }
    }
}
