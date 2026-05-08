using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMFINAV.SchemeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketHoliday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketHolidays", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketHolidays_HolidayDate",
                table: "MarketHolidays",
                column: "HolidayDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketHolidays");
        }
    }
}
