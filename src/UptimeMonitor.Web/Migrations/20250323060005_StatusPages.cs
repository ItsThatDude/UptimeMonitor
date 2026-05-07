using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class StatusPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Monitor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MonitorType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monitor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusPageMonitorGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StatusPageId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPageMonitorGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusPageMonitorGroup_StatusPages_StatusPageId",
                        column: x => x.StatusPageId,
                        principalTable: "StatusPages",
                        principalColumn: "Id");
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

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageMonitorGroup_StatusPageId",
                table: "StatusPageMonitorGroup",
                column: "StatusPageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitorStatusPageMonitorGroup");

            migrationBuilder.DropTable(
                name: "Monitor");

            migrationBuilder.DropTable(
                name: "StatusPageMonitorGroup");

            migrationBuilder.DropTable(
                name: "StatusPages");
        }
    }
}
