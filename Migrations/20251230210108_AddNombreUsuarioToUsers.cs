using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACCOB.Migrations
{
    /// <inheritdoc />
    public partial class AddNombreUsuarioToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreUsuario",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreUsuario",
                table: "AspNetUsers");
        }
    }
}
