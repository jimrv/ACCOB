using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACCOB.Models
{
    public class RegistroVenta
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        // Guardamos IDs para relación, pero también texto por si el plan cambia en el futuro
        public string ZonaNombre { get; set; }
        public string PlanNombre { get; set; }
        public string VelocidadContratada { get; set; }
        public decimal PrecioFinal { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    }
}