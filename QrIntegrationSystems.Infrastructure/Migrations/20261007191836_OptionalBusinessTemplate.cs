using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QrIntegrationSystems.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptionalBusinessTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Templates_TemplateId",
                schema: "public",
                table: "Businesses");

            migrationBuilder.AlterColumn<int>(
                name: "TemplateId",
                schema: "public",
                table: "Businesses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Templates_TemplateId",
                schema: "public",
                table: "Businesses",
                column: "TemplateId",
                principalSchema: "public",
                principalTable: "Templates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Templates_TemplateId",
                schema: "public",
                table: "Businesses");

            migrationBuilder.AlterColumn<int>(
                name: "TemplateId",
                schema: "public",
                table: "Businesses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Templates_TemplateId",
                schema: "public",
                table: "Businesses",
                column: "TemplateId",
                principalSchema: "public",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
