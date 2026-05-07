using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWorkerRegistrationTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Hostname",
                table: "WorkerConfigurations",
                newName: "Secret");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "WorkerConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "WorkerConfigurations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WorkerConfigurations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "WorkerConfigurations");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "WorkerConfigurations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "WorkerConfigurations");

            migrationBuilder.RenameColumn(
                name: "Secret",
                table: "WorkerConfigurations",
                newName: "Hostname");
        }
    }
}
