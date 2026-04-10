using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMFINAV.SchemeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundCodeToSchemeEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FundCode",
                table: "SchemeEnrollments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FundName",
                table: "SchemeEnrollments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SchemeEnrollments_FundCode",
                table: "SchemeEnrollments",
                column: "FundCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchemeEnrollments_FundCode",
                table: "SchemeEnrollments");

            migrationBuilder.DropColumn(
                name: "FundCode",
                table: "SchemeEnrollments");

            migrationBuilder.DropColumn(
                name: "FundName",
                table: "SchemeEnrollments");
        }
    }
}
