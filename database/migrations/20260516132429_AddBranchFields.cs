using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistanceManagementSystem.database.migrations
{
    /// <inheritdoc />
    public partial class AddBranchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Organizations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "AssistanceRequests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BeneficiaryTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeneficiaryId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromBranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToBranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryTransfers_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BeneficiaryTransfers_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxPayout = table.Column<decimal>(type: "TEXT", nullable: true),
                    WorkingHours = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchSettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssistanceRequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewAudits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewAudits_AssistanceRequests_AssistanceRequestId",
                        column: x => x.AssistanceRequestId,
                        principalTable: "AssistanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BranchId",
                table: "Projects",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_BranchId",
                table: "Organizations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequests_BranchId",
                table: "AssistanceRequests",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryTransfers_BeneficiaryId",
                table: "BeneficiaryTransfers",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryTransfers_PerformedByUserId",
                table: "BeneficiaryTransfers",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchSettings_BranchId",
                table: "BranchSettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAudits_AssistanceRequestId",
                table: "ReviewAudits",
                column: "AssistanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAudits_UserId",
                table: "ReviewAudits",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssistanceRequests_Branches_BranchId",
                table: "AssistanceRequests",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Branches_BranchId",
                table: "Organizations",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Branches_BranchId",
                table: "Projects",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssistanceRequests_Branches_BranchId",
                table: "AssistanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Branches_BranchId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Branches_BranchId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "BeneficiaryTransfers");

            migrationBuilder.DropTable(
                name: "BranchSettings");

            migrationBuilder.DropTable(
                name: "ReviewAudits");

            migrationBuilder.DropIndex(
                name: "IX_Projects_BranchId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_BranchId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_AssistanceRequests_BranchId",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AssistanceRequests");
        }
    }
}
