using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UptimeMonitor.Web.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitorConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Target = table.Column<string>(type: "text", nullable: false),
                    Interval = table.Column<int>(type: "integer", nullable: false),
                    Settings = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkerConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hostname = table.Column<string>(type: "text", nullable: false),
                    ConfigUpdateInterval = table.Column<int>(type: "integer", nullable: false),
                    LastCheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitorResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkerConfigurationId = table.Column<int>(type: "integer", nullable: false),
                    MonitorConfigurationId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ResponseTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitorResults_MonitorConfigurations_MonitorConfigurationId",
                        column: x => x.MonitorConfigurationId,
                        principalTable: "MonitorConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonitorResults_WorkerConfigurations_WorkerConfigurationId",
                        column: x => x.WorkerConfigurationId,
                        principalTable: "WorkerConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitorResults_MonitorConfigurationId",
                table: "MonitorResults",
                column: "MonitorConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitorResults_WorkerConfigurationId",
                table: "MonitorResults",
                column: "WorkerConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitorResults");

            migrationBuilder.DropTable(
                name: "MonitorConfigurations");

            migrationBuilder.DropTable(
                name: "WorkerConfigurations");
        }
    }
}
