using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RevertChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagamentoProdotti");

            migrationBuilder.DropTable(
                name: "Pagamenti");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagamenti",
                columns: table => new
                {
                    IdPagamento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PayPalOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamenti", x => x.IdPagamento);
                    table.ForeignKey(
                        name: "FK_Pagamenti_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagamentoProdotti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PagamentoId = table.Column<int>(type: "int", nullable: false),
                    ProdottoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentoProdotti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagamentoProdotti_Pagamenti_PagamentoId",
                        column: x => x.PagamentoId,
                        principalTable: "Pagamenti",
                        principalColumn: "IdPagamento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagamentoProdotti_Prodotti_ProdottoId",
                        column: x => x.ProdottoId,
                        principalTable: "Prodotti",
                        principalColumn: "IdProdotto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagamenti_ClienteId",
                table: "Pagamenti",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentoProdotti_PagamentoId",
                table: "PagamentoProdotti",
                column: "PagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentoProdotti_ProdottoId",
                table: "PagamentoProdotti",
                column: "ProdottoId");
        }
    }
}
