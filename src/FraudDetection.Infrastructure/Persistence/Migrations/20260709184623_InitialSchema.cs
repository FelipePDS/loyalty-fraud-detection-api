using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudDetection.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FraudAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PatternType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DetectionSource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvolvedTransactionIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FraudReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    WindowFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindowTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MarkdownContent = table.Column<string>(type: "text", nullable: false),
                    AlertCount = table.Column<int>(type: "integer", nullable: false),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransactionCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FraudAlerts_CreatedAt",
                table: "FraudAlerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FraudAlerts_CustomerId",
                table: "FraudAlerts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudAlerts_Severity_Status",
                table: "FraudAlerts",
                columns: new[] { "Severity", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FraudAlerts_Status",
                table: "FraudAlerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FraudReports_CustomerId_GeneratedAt",
                table: "FraudReports",
                columns: new[] { "CustomerId", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FraudReports_GeneratedAt",
                table: "FraudReports",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSnapshots_CreatedAt",
                table: "TransactionSnapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSnapshots_CustomerId_TransactionCreatedAt",
                table: "TransactionSnapshots",
                columns: new[] { "CustomerId", "TransactionCreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_TransactionSnapshots_OriginalTransactionId",
                table: "TransactionSnapshots",
                column: "OriginalTransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FraudAlerts");

            migrationBuilder.DropTable(
                name: "FraudReports");

            migrationBuilder.DropTable(
                name: "TransactionSnapshots");
        }
    }
}
