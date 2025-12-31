using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACCOB.Data;

namespace ACCOB.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        public string Email { get; set; }

        public string Telefono { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Atendido
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relación con el Asesor (ApplicationUser)
        [Required(ErrorMessage = "Debes asignar un asesor")]
        public string AsesorId { get; set; }
        
        [ForeignKey("AsesorId")]
        public virtual ApplicationUser? Asesor { get; set; }
    }
}