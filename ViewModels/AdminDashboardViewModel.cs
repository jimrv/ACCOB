using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACCOB.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalRoles { get; set; }
        public int TotalClientes { get; set; }
        public int AsesoresEnLinea { get; set; }
        public string UsuarioActual { get; set; }
    }
}