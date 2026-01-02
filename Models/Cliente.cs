using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACCOB.Data;

namespace ACCOB.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^\d{8,12}$", ErrorMessage = "El DNI debe tener entre 8 y 12 dígitos")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [RegularExpression(@"^\d{9,15}$", ErrorMessage = "El teléfono debe tener entre 9 y 15 dígitos")]
        public string Telefono { get; set; }

        public string Direccion { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public string? AsesorId { get; set; }

        [ForeignKey("AsesorId")]
        public virtual ApplicationUser? Asesor { get; set; }
    }
}