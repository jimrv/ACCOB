using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACCOB.Models
{
    public class Zona
    {
        public int Id { get; set; }
        public string Nombre { get; set; } // "Zona 1", "Zona Digital", etc.
        public virtual ICollection<PlanWin> Planes { get; set; }
    }
}