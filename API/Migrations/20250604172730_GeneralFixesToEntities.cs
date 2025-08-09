using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class GeneralFixesToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelErrorDetails");

            migrationBuilder.DropTable(
                name: "ExcelImportMappingDetails");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "ExcelMappingHeaders");

            migrationBuilder.DropColumn(
                name: "ImportedAt",
                table: "ExcelLogs");

            migrationBuilder.DropColumn(
                name: "ImportedBy",
                table: "ExcelLogs");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExcelMappingHeaders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ExcelLogDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcelLogId = table.Column<int>(type: "int", nullable: false),
                    ExcelColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelLogDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcelLogDetails_ExcelLogs_ExcelLogId",
                        column: x => x.ExcelLogId,
                        principalTable: "ExcelLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcelMappingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcelMappingHeaderId = table.Column<int>(type: "int", nullable: false),
                    ExcelColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelMappingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcelMappingDetails_ExcelMappingHeaders_ExcelMappingHeaderId",
                        column: x => x.ExcelMappingHeaderId,
                        principalTable: "ExcelMappingHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcelMappingHeaders_UserId",
                table: "ExcelMappingHeaders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelLogDetails_ExcelLogId",
                table: "ExcelLogDetails",
                column: "ExcelLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelMappingDetails_ExcelMappingHeaderId",
                table: "ExcelMappingDetails",
                column: "ExcelMappingHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExcelMappingHeaders_AspNetUsers_UserId",
                table: "ExcelMappingHeaders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExcelMappingHeaders_AspNetUsers_UserId",
                table: "ExcelMappingHeaders");

            migrationBuilder.DropTable(
                name: "ExcelLogDetails");

            migrationBuilder.DropTable(
                name: "ExcelMappingDetails");

            migrationBuilder.DropIndex(
                name: "IX_ExcelMappingHeaders_UserId",
                table: "ExcelMappingHeaders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExcelMappingHeaders");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "ExcelMappingHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportedAt",
                table: "ExcelLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ImportedBy",
                table: "ExcelLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ExcelErrorDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcelLogId = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExcelColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsWarning = table.Column<bool>(type: "bit", nullable: false),
                    RawValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowNumber = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ExcelImportMappingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcelMappingHeaderId = table.Column<int>(type: "int", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExcelColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_ExcelErrorDetails_ExcelLogId",
                table: "ExcelErrorDetails",
                column: "ExcelLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelImportMappingDetails_ExcelMappingHeaderId",
                table: "ExcelImportMappingDetails",
                column: "ExcelMappingHeaderId");
        }
    }
}
