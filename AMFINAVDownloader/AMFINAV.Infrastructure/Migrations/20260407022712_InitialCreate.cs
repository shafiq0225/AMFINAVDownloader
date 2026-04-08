using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMFINAV.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NavFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NavDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<int>(type: "int", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    RecordCount = table.Column<int>(type: "int", nullable: false),
                    Checksum = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NavFiles_NavDate",
                table: "NavFiles",
                column: "NavDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NavFiles");
        }
    }
}
