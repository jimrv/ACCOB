using Microsoft.AspNetCore.Identity;

namespace ACCOB.Data;

public class ApplicationUser : IdentityUser
{
    public string? NombreUsuario { get; set; }
    public string? Nombre { get; set; }
    public string? Dni { get; set; }
    public string? Celular { get; set; }

    // ... tus propiedades existentes (Nombre, Dni, etc.)
    public bool EstaConectado { get; set; } = false;
}
