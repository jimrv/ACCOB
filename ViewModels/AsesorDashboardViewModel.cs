using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACCOB.Models;

namespace ACCOB.ViewModels
{
    public class AsesorDashboardViewModel
    {
        public string Busqueda { get; set; } = string.Empty;
        
        public int TotalClientes { get; set; }
        public int ClientesPendientes { get; set; }
        public int ClientesAtendidos { get; set; }

        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}