using Microsoft.AspNetCore.Identity;

namespace ACCOB.Data;

public class ApplicationUser : IdentityUser
{
    // Opcional: si quieres campos extra
    public string? NombreUsuario { get; set; }
    public string? Nombre { get; set; }
    public string? Dni { get; set; }
    public string? Celular { get; set; }
}
