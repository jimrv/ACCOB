using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Añadido para [Key]
using System.ComponentModel.DataAnnotations.Schema; // Añadido para [ForeignKey]

namespace ACCOB.Models
{
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } // Ahora lo reconocerá

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoPagado { get; set; }

        public DateTime FechaOperacion { get; set; } = DateTime.UtcNow;

        public string TipoPago { get; set; }
        public string MetodoPago { get; set; }
    }
}