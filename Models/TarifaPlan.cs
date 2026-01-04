using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACCOB.Models
{
    public class TarifaPlan
    {
        public int Id { get; set; }
        public string Velocidad { get; set; } // "200 Mbps", "1 Gbps"
        public decimal PrecioRegular { get; set; }
        public decimal PrecioPromocional { get; set; }
        public string DescripcionDescuento { get; set; } // "50% por 3 meses"

        public int PlanWinId { get; set; }
        [ForeignKey("PlanWinId")]
        public virtual PlanWin Plan { get; set; }
    }
}