using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistanceManagementSystem.database.migrations
{
    /// <inheritdoc />
    public partial class AddBilingualNamesAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullNameAr",
                table: "Beneficiaries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullNameEn",
                table: "Beneficiaries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "Assistances",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryNameAr",
                table: "AssistanceRequests",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryNameEn",
                table: "AssistanceRequests",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "AssistanceRequests",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterNameAr",
                table: "AssistanceRequests",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequesterNameEn",
                table: "AssistanceRequests",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "FullNameAr",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "FullNameEn",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "Assistances");

            migrationBuilder.DropColumn(
                name: "BeneficiaryNameAr",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "BeneficiaryNameEn",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "RequesterNameAr",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "RequesterNameEn",
                table: "AssistanceRequests");
        }
    }
}
