namespace ACCOB.ViewModels
{
    public class ClienteListViewModel
    {
        public IEnumerable<ACCOB.Models.Cliente> Clientes { get; set; }
        
        // Propiedades para los filtros
        public string? Nombre { get; set; }
        public string? Estado { get; set; }
        public string? AsesorId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}