using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QrIntegrationSystems.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedAttemptsOtpCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                schema: "public",
                table: "OTPCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                schema: "public",
                table: "OTPCodes");
        }
    }
}
