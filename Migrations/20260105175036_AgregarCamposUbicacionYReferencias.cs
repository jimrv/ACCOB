using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACCOB.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposUbicacionYReferencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "Clientes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Distrito",
                table: "Clientes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumRef1",
                table: "Clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumRef2",
                table: "Clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Clientes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Distrito",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumRef1",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumRef2",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Clientes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
