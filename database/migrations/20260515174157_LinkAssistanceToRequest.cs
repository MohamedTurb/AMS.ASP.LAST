using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistanceManagementSystem.database.migrations
{
    /// <inheritdoc />
    public partial class LinkAssistanceToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssistanceRequestId",
                table: "Assistances",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_AssistanceRequestId",
                table: "Assistances",
                column: "AssistanceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assistances_AssistanceRequests_AssistanceRequestId",
                table: "Assistances",
                column: "AssistanceRequestId",
                principalTable: "AssistanceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assistances_AssistanceRequests_AssistanceRequestId",
                table: "Assistances");

            migrationBuilder.DropIndex(
                name: "IX_Assistances_AssistanceRequestId",
                table: "Assistances");

            migrationBuilder.DropColumn(
                name: "AssistanceRequestId",
                table: "Assistances");
        }
    }
}
