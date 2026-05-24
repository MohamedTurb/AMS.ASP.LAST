using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistanceManagementSystem.database.migrations
{
    /// <inheritdoc />
    public partial class AddAidCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AidCategoryId",
                table: "Beneficiaries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AidCategoryDetails",
                table: "AssistanceRequests",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AidCategoryId",
                table: "AssistanceRequests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AidCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameAr = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowCustomText = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentAidCategoryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AidCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AidCategories_AidCategories_ParentAidCategoryId",
                        column: x => x.ParentAidCategoryId,
                        principalTable: "AidCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_AidCategoryId",
                table: "Beneficiaries",
                column: "AidCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequests_AidCategoryId",
                table: "AssistanceRequests",
                column: "AidCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AidCategories_ParentAidCategoryId",
                table: "AidCategories",
                column: "ParentAidCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssistanceRequests_AidCategories_AidCategoryId",
                table: "AssistanceRequests",
                column: "AidCategoryId",
                principalTable: "AidCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_AidCategories_AidCategoryId",
                table: "Beneficiaries",
                column: "AidCategoryId",
                principalTable: "AidCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssistanceRequests_AidCategories_AidCategoryId",
                table: "AssistanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_AidCategories_AidCategoryId",
                table: "Beneficiaries");

            migrationBuilder.DropTable(
                name: "AidCategories");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_AidCategoryId",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_AssistanceRequests_AidCategoryId",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "AidCategoryId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "AidCategoryDetails",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "AidCategoryId",
                table: "AssistanceRequests");
        }
    }
}
