using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ACCOB.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoTablasPlanesWin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosVentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    ZonaNombre = table.Column<string>(type: "text", nullable: false),
                    PlanNombre = table.Column<string>(type: "text", nullable: false),
                    VelocidadContratada = table.Column<string>(type: "text", nullable: false),
                    PrecioFinal = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosVentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosVentas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zonas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesWin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    ZonaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesWin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesWin_Zonas_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "Zonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TarifasPlan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Velocidad = table.Column<string>(type: "text", nullable: false),
                    PrecioRegular = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioPromocional = table.Column<decimal>(type: "numeric", nullable: false),
                    DescripcionDescuento = table.Column<string>(type: "text", nullable: false),
                    PlanWinId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TarifasPlan_PlanesWin_PlanWinId",
                        column: x => x.PlanWinId,
                        principalTable: "PlanesWin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanesWin_ZonaId",
                table: "PlanesWin",
                column: "ZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosVentas_ClienteId",
                table: "RegistrosVentas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasPlan_PlanWinId",
                table: "TarifasPlan",
                column: "PlanWinId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosVentas");

            migrationBuilder.DropTable(
                name: "TarifasPlan");

            migrationBuilder.DropTable(
                name: "PlanesWin");

            migrationBuilder.DropTable(
                name: "Zonas");
        }
    }
}
