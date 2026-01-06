using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ACCOB.Data;
using ACCOB.Models;
using ACCOB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ACCOB.Controllers
{
    [Authorize(Roles = "Asesor")]
    public class AsesorController : Controller
    {
        private readonly ILogger<AsesorController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AsesorController(
            ILogger<AsesorController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // 1. DASHBOARD: Estadísticas y últimos clientes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var hoy = DateTime.UtcNow.Date;
            var mañana = hoy.AddDays(1);

            var query = _context.Clientes.Where(c => c.AsesorId == userId);

            var model = new AsesorDashboardViewModel
            {
                TotalClientes = await query.CountAsync(),
                ClientesPendientes = await query.CountAsync(c => c.Estado == "Pendiente"),
                ClientesEnGestion = await query.CountAsync(c => c.Estado == "En Gestión"),
                ClientesCerrados = await query.CountAsync(c => c.Estado == "Cerrado"),

                // Los clientes cargados aquí ya podrán usar .NombreCompleto en la vista
                Clientes = await query.OrderByDescending(c => c.FechaRegistro).Take(5).ToListAsync(),

                NuevasAsignacionesHoy = await query.CountAsync(c => c.FechaRegistro >= hoy && c.Estado == "Pendiente"),

                RecordatoriosHoy = await _context.RegistroLlamadas
                    .Include(r => r.Cliente)
                    .Where(r => r.AsesorId == userId &&
                                r.ProximaLlamada >= hoy &&
                                r.ProximaLlamada < mañana)
                    .OrderBy(r => r.ProximaLlamada)
                    .ToListAsync()
            };

            return View(model);
        }

        // 2. LISTADO: Buscador actualizado para Nombres y Apellidos
        [HttpGet]
        public async Task<IActionResult> Listado(string buscar, string estado)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Clientes.Where(c => c.AsesorId == userId).AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                var b = buscar.ToLower().Trim();
                // Buscamos coincidencia en DNI, Nombre o Apellido
                query = query.Where(c => c.Dni.Contains(b)
                                      || c.Nombre.ToLower().Contains(b)
                                      || c.Apellido.ToLower().Contains(b));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                // Normalización de estado para que funcione con "En Gestión"
                query = query.Where(c => c.Estado.ToLower() == estado.ToLower().Trim());
            }

            var clientes = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync();

            ViewBag.BusquedaActual = buscar;
            ViewBag.EstadoActual = estado;

            return View(clientes);
        }

        // 3. DETALLE: Ver expediente completo
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var cliente = await _context.Clientes
                .Include(c => c.Llamadas)
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(m => m.Id == id && m.AsesorId == userId);

            if (cliente == null) return NotFound();

            ViewBag.Zonas = await _context.Zonas.ToListAsync();

            return View(cliente);
        }

        [HttpGet]
        public async Task<JsonResult> GetPlanesPorZona(int zonaId)
        {
            var planes = await _context.PlanesWin
                .Where(p => p.ZonaId == zonaId)
                .Select(p => new { id = p.Id, nombre = p.Nombre })
                .ToListAsync();
            return Json(planes);
        }

        [HttpGet]
        public async Task<JsonResult> GetTarifasPorPlan(int planId)
        {
            var tarifas = await _context.TarifasPlan
                .Where(t => t.PlanWinId == planId)
                .Select(t => new
                {
                    id = t.Id,
                    velocidad = t.Velocidad,
                    precio = t.PrecioPromocional,
                    descuento = t.DescripcionDescuento
                })
                .ToListAsync();
            return Json(tarifas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarLlamada(int clienteId, string resultado, string observaciones, DateTime? proximaLlamada, int? tarifaId)
        {
            var userId = _userManager.GetUserId(User);

            DateTime? fechaRecordatorio = null;
            if (proximaLlamada.HasValue)
            {
                fechaRecordatorio = DateTime.SpecifyKind(proximaLlamada.Value, DateTimeKind.Utc);
            }

            var nuevaLlamada = new RegistroLlamada
            {
                ClienteId = clienteId,
                AsesorId = userId,
                Resultado = resultado,
                Observaciones = observaciones,
                FechaLlamada = DateTime.UtcNow,
                ProximaLlamada = fechaRecordatorio
            };

            try
            {
                _context.RegistroLlamadas.Add(nuevaLlamada);

                var cliente = await _context.Clientes.FindAsync(clienteId);
                if (cliente != null)
                {
                    if (resultado == "Venta Cerrada")
                    {
                        cliente.Estado = "Cerrado";

                        if (tarifaId.HasValue)
                        {
                            var tarifa = await _context.TarifasPlan
                                .Include(t => t.Plan)
                                .ThenInclude(p => p.Zona)
                                .FirstOrDefaultAsync(t => t.Id == tarifaId.Value);

                            if (tarifa != null)
                            {
                                var venta = new RegistroVenta
                                {
                                    ClienteId = clienteId,
                                    ZonaNombre = tarifa.Plan.Zona.Nombre,
                                    PlanNombre = tarifa.Plan.Nombre,
                                    VelocidadContratada = tarifa.Velocidad,
                                    PrecioFinal = tarifa.PrecioPromocional,
                                    FechaVenta = DateTime.UtcNow
                                };
                                _context.RegistrosVentas.Add(venta);
                            }
                        }
                    }
                    else if (cliente.Estado == "Pendiente")
                    {
                        cliente.Estado = "En Gestión";
                    }

                    _context.Update(cliente);
                }

                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Gestión registrada correctamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al registrar: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Details), new { id = clienteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarDireccion(int id, string? direccion)
        {
            var userId = _userManager.GetUserId(User);
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.AsesorId == userId);

            if (cliente != null)
            {
                // Si direccion es nulo o espacios en blanco, guardará null en la BD
                cliente.Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();

                _context.Update(cliente);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Dirección actualizada correctamente.";
                TempData["TipoMensaje"] = "success";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarCliente(int id)
        {
            var userId = _userManager.GetUserId(User);
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.AsesorId == userId);

            if (cliente != null)
            {
                cliente.Estado = "Cerrado";
                _context.Update(cliente);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Expediente cerrado con éxito.";
                TempData["TipoMensaje"] = "success";
            }
            else
            {
                TempData["Mensaje"] = "No se pudo actualizar el cliente.";
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [Route("Asesor/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}