using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddExcelEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcelMappingHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeMapping = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "char(1)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelMappingHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExcelImportMappingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcelMappingHeaderId = table.Column<int>(type: "int", nullable: false),
                    ExcelColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelImportMappingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcelImportMappingDetails_ExcelMappingHeaders_ExcelMappingHeaderId",
                        column: x => x.ExcelMappingHeaderId,
                        principalTable: "ExcelMappingHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcelLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcelMappingHeaderId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "char(1)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    SuccessRows = table.Column<int>(type: "int", nullable: false),
                    ErrorRows = table.Column<int>(type: "int", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcelLogs_ExcelMappingHeaders_ExcelMappingHeaderId",
                        column: x => x.ExcelMappingHeaderId,
                        principalTable: "ExcelMappingHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcelErrorDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcelLogId = table.Column<int>(type: "int", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    ExcelColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelErrorDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcelErrorDetails_ExcelLogs_ExcelLogId",
                        column: x => x.ExcelLogId,
                        principalTable: "ExcelLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcelErrorDetails_ExcelLogId",
                table: "ExcelErrorDetails",
                column: "ExcelLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportMappingDetails_ExcelMappingHeaderId",
                table: "ExcelImportMappingDetails",
                column: "ExcelMappingHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelLogs_ExcelMappingHeaderId",
                table: "ExcelLogs",
                column: "ExcelMappingHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelErrorDetails");

            migrationBuilder.DropTable(
                name: "ExcelImportMappingDetails");

            migrationBuilder.DropTable(
                name: "ExcelLogs");

            migrationBuilder.DropTable(
                name: "ExcelMappingHeaders");
        }
    }
}
