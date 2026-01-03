using ACCOB.Models;
using System.Collections.Generic;

namespace ACCOB.ViewModels
{
    public class AsesorDashboardViewModel
    {
        public int TotalClientes { get; set; }
        public int ClientesPendientes { get; set; }
        public int ClientesEnGestion { get; set; } 
        public int ClientesCerrados { get; set; }  
        
        public List<Cliente> Clientes { get; set; }
        public int NuevasAsignacionesHoy { get; set; }
        public List<RegistroLlamada> RecordatoriosHoy { get; set; } = new List<RegistroLlamada>();
    }
}