using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMFINAV.SchemeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchemeEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SchemeName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemeEnrollments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchemeEnrollments_SchemeCode",
                table: "SchemeEnrollments",
                column: "SchemeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchemeEnrollments");
        }
    }
}
