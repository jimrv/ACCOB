using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACCOB.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarEliminacionAsesorSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_AspNetUsers_AsesorId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistroLlamadas_AspNetUsers_AsesorId",
                table: "RegistroLlamadas");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_AspNetUsers_AsesorId",
                table: "Clientes",
                column: "AsesorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroLlamadas_AspNetUsers_AsesorId",
                table: "RegistroLlamadas",
                column: "AsesorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_AspNetUsers_AsesorId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistroLlamadas_AspNetUsers_AsesorId",
                table: "RegistroLlamadas");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_AspNetUsers_AsesorId",
                table: "Clientes",
                column: "AsesorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroLlamadas_AspNetUsers_AsesorId",
                table: "RegistroLlamadas",
                column: "AsesorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
