using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class StatusPagesCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusPageMonitorGroup_StatusPages_StatusPageId",
                table: "StatusPageMonitorGroup");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusPageMonitorGroup_StatusPages_StatusPageId",
                table: "StatusPageMonitorGroup",
                column: "StatusPageId",
                principalTable: "StatusPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusPageMonitorGroup_StatusPages_StatusPageId",
                table: "StatusPageMonitorGroup");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusPageMonitorGroup_StatusPages_StatusPageId",
                table: "StatusPageMonitorGroup",
                column: "StatusPageId",
                principalTable: "StatusPages",
                principalColumn: "Id");
        }
    }
}
