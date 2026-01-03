using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACCOB.Data;
using ACCOB.Models;

namespace ACCOB.Models
{
    public class RegistroLlamada
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        // Enlace con el asesor de la sesión
        public string AsesorId { get; set; }
        [ForeignKey("AsesorId")]
        public virtual ApplicationUser Asesor { get; set; }

        public DateTime FechaLlamada { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El resultado es obligatorio")]
        public string Resultado { get; set; } // Ej: Migración, No contesta, No interesado, Interesado, etc.
        public string Observaciones { get; set; }
        public DateTime? ProximaLlamada { get; set; } // Recordatorio
    }
}