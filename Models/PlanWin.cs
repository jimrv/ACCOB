using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACCOB.Models
{
    public class PlanWin
    {
        public int Id { get; set; }
        public string Nombre { get; set; } // "WIN Gamer", "WIN Hogar"
        public int ZonaId { get; set; }
        [ForeignKey("ZonaId")]
        public virtual Zona Zona { get; set; }
        public virtual ICollection<TarifaPlan> Tarifas { get; set; }
    }
}