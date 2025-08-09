using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixTablesInDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrelli_Clienti_ClienteId",
                table: "Carrelli");

            migrationBuilder.DropIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli");

            migrationBuilder.AlterColumn<string>(
                name: "ClienteId",
                table: "Carrelli",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carrelli_AspNetUsers_ClienteId",
                table: "Carrelli",
                column: "ClienteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrelli_AspNetUsers_ClienteId",
                table: "Carrelli");

            migrationBuilder.DropIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli");

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Carrelli",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Carrelli_ClienteId",
                table: "Carrelli",
                column: "ClienteId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Carrelli_Clienti_ClienteId",
                table: "Carrelli",
                column: "ClienteId",
                principalTable: "Clienti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
