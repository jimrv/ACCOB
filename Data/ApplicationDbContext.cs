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
    public DbSet<Pago> Pagos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 1. Conservar la configuración base de Identity (Esencial)
        base.OnModelCreating(builder);

        // 2. Configuración para que las llamadas se mantengan vivas al eliminar al asesor
        builder.Entity<RegistroLlamada>()
            .HasOne(l => l.Asesor)
            .WithMany()
            .HasForeignKey(l => l.AsesorId)
            .OnDelete(DeleteBehavior.SetNull);

        // 3. Configuración para que los clientes pasen a "Sin Asignar" (CORREGIDO)
        builder.Entity<Cliente>()
            .HasOne(c => c.Asesor)
            .WithMany() 
            .HasForeignKey(c => c.AsesorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
