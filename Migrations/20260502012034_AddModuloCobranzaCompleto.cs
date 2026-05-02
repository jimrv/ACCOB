using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACCOB.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloCobranzaCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pago_Clientes_ClienteId",
                table: "Pago");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pago",
                table: "Pago");

            migrationBuilder.RenameTable(
                name: "Pago",
                newName: "Pagos");

            migrationBuilder.RenameIndex(
                name: "IX_Pago_ClienteId",
                table: "Pagos",
                newName: "IX_Pagos_ClienteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pagos",
                table: "Pagos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Clientes_ClienteId",
                table: "Pagos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Clientes_ClienteId",
                table: "Pagos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pagos",
                table: "Pagos");

            migrationBuilder.RenameTable(
                name: "Pagos",
                newName: "Pago");

            migrationBuilder.RenameIndex(
                name: "IX_Pagos_ClienteId",
                table: "Pago",
                newName: "IX_Pago_ClienteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pago",
                table: "Pago",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pago_Clientes_ClienteId",
                table: "Pago",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
