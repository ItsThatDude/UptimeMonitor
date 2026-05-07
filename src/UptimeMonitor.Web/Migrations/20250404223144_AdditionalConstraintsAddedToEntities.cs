using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalConstraintsAddedToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonitorConfigurationStatusPageMonitorGroup_MonitorConfigura~",
                table: "MonitorConfigurationStatusPageMonitorGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_MonitorConfigurationStatusPageMonitorGroup_StatusPageMonito~",
                table: "MonitorConfigurationStatusPageMonitorGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MonitorConfigurationStatusPageMonitorGroup",
                table: "MonitorConfigurationStatusPageMonitorGroup");

            migrationBuilder.RenameTable(
                name: "MonitorConfigurationStatusPageMonitorGroup",
                newName: "StatusPageMonitorGroupMonitors");

            migrationBuilder.RenameIndex(
                name: "IX_MonitorConfigurationStatusPageMonitorGroup_StatusPageMonito~",
                table: "StatusPageMonitorGroupMonitors",
                newName: "IX_StatusPageMonitorGroupMonitors_StatusPageMonitorGroupId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WorkerConfigurations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigUpdateInterval",
                table: "WorkerConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 60,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusPageMonitorGroupMonitors",
                table: "StatusPageMonitorGroupMonitors",
                columns: new[] { "MonitorsId", "StatusPageMonitorGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkerConfigurations_Name",
                table: "WorkerConfigurations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusPages_Slug",
                table: "StatusPages",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StatusPageMonitorGroupMonitors_MonitorConfigurations_Monito~",
                table: "StatusPageMonitorGroupMonitors",
                column: "MonitorsId",
                principalTable: "MonitorConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatusPageMonitorGroupMonitors_StatusPageMonitorGroup_Statu~",
                table: "StatusPageMonitorGroupMonitors",
                column: "StatusPageMonitorGroupId",
                principalTable: "StatusPageMonitorGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusPageMonitorGroupMonitors_MonitorConfigurations_Monito~",
                table: "StatusPageMonitorGroupMonitors");

            migrationBuilder.DropForeignKey(
                name: "FK_StatusPageMonitorGroupMonitors_StatusPageMonitorGroup_Statu~",
                table: "StatusPageMonitorGroupMonitors");

            migrationBuilder.DropIndex(
                name: "IX_WorkerConfigurations_Name",
                table: "WorkerConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_StatusPages_Slug",
                table: "StatusPages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusPageMonitorGroupMonitors",
                table: "StatusPageMonitorGroupMonitors");

            migrationBuilder.RenameTable(
                name: "StatusPageMonitorGroupMonitors",
                newName: "MonitorConfigurationStatusPageMonitorGroup");

            migrationBuilder.RenameIndex(
                name: "IX_StatusPageMonitorGroupMonitors_StatusPageMonitorGroupId",
                table: "MonitorConfigurationStatusPageMonitorGroup",
                newName: "IX_MonitorConfigurationStatusPageMonitorGroup_StatusPageMonito~");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WorkerConfigurations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "ConfigUpdateInterval",
                table: "WorkerConfigurations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 60);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonitorConfigurationStatusPageMonitorGroup",
                table: "MonitorConfigurationStatusPageMonitorGroup",
                columns: new[] { "MonitorsId", "StatusPageMonitorGroupId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MonitorConfigurationStatusPageMonitorGroup_MonitorConfigura~",
                table: "MonitorConfigurationStatusPageMonitorGroup",
                column: "MonitorsId",
                principalTable: "MonitorConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MonitorConfigurationStatusPageMonitorGroup_StatusPageMonito~",
                table: "MonitorConfigurationStatusPageMonitorGroup",
                column: "StatusPageMonitorGroupId",
                principalTable: "StatusPageMonitorGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
