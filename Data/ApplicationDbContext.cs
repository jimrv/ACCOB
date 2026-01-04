using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ACCOB.Models;

namespace ACCOB.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<RegistroLlamada> RegistroLlamadas { get; set; }
    public DbSet<Zona> Zonas { get; set; }
    public DbSet<PlanWin> PlanesWin { get; set; }
    public DbSet<TarifaPlan> TarifasPlan { get; set; }
    public DbSet<RegistroVenta> RegistrosVentas { get; set; }
}
