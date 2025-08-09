using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixTablesInDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli");

            migrationBuilder.CreateIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli",
                column: "ClienteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli");

            migrationBuilder.CreateIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli",
                column: "ClienteId");
        }
    }
}
