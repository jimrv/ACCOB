using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACCOB.Models;

namespace ACCOB.ViewModels
{
    public class NotificacionesViewModel
    {
        public int NuevasAsignaciones { get; set; }
        public List<Cliente> ProximasLlamadasHoy { get; set; }
        public List<RegistroLlamada> RecordatoriosVencidos { get; set; }
    }
}