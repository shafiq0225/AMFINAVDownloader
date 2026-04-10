using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMFINAV.SchemeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetailedSchemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FundName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SchemeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SchemeName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Nav = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NavDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailedSchemes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailedSchemes_FundCode",
                table: "DetailedSchemes",
                column: "FundCode");

            migrationBuilder.CreateIndex(
                name: "IX_DetailedSchemes_SchemeCode_NavDate",
                table: "DetailedSchemes",
                columns: new[] { "SchemeCode", "NavDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetailedSchemes");
        }
    }
}
