using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;
using ACCOB.Data;
using ACCOB.ViewModels;
using ACCOB.Models;

namespace ACCOB.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AdminController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        // GET: Panel principal
        public IActionResult Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsuarios = _userManager.Users.Count(),
                TotalRoles = _roleManager.Roles.Count(),
                TotalClientes = _context.Clientes.Count(),
                UsuarioActual = User.Identity.Name
            };

            return View(model);
        }

        // GET: Formulario Crear Asesor
        public IActionResult CrearAsesor()
        {
            return View();
        }

        // POST: Crear Asesor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAsesor(CrearAsesorViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verificar si ya existe un usuario con ese DNI
                var usuarioExistente = await _userManager.FindByNameAsync(model.Dni);
                if (usuarioExistente != null)
                {
                    ModelState.AddModelError("Dni", "Ya existe un usuario con este DNI.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Dni,  // El DNI será el nombre de usuario
                    Email = $"{model.Dni}@accob.com",  // Email generado automáticamente
                    EmailConfirmed = true,
                    Nombre = model.Nombre,
                    Dni = model.Dni,
                    Celular = model.Celular,
                    PhoneNumber = model.Celular
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Asignar rol de Asesor
                    await _userManager.AddToRoleAsync(user, "Asesor");

                    TempData["Mensaje"] = $"Asesor {model.Nombre} creado exitosamente.";
                    TempData["TipoMensaje"] = "success";
                    return RedirectToAction("Usuarios");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Lista de usuarios
        public IActionResult Usuarios()
        {
            var usuarios = _userManager.Users.ToList();
            return View(usuarios);
        }

        // GET: Editar Asesor
        public async Task<IActionResult> EditarAsesor(string id)
        {
            if (id == null) return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var model = new EditarAsesorViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Dni = usuario.Dni,
                Celular = usuario.Celular
            };

            return View(model);
        }

        // POST: Editar Asesor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAsesor(EditarAsesorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                // VALIDACIÓN: Verificar si el nuevo DNI ya lo tiene otro usuario
                var usuarioConMismoDni = await _userManager.FindByNameAsync(model.Dni);
                if (usuarioConMismoDni != null && usuarioConMismoDni.Id != user.Id)
                {
                    ModelState.AddModelError("Dni", "Este DNI ya está registrado por otro usuario.");
                    return View(model);
                }

                // Actualizar datos básicos
                user.Nombre = model.Nombre;
                user.Dni = model.Dni;
                user.UserName = model.Dni;
                user.Celular = model.Celular;
                user.PhoneNumber = model.Celular;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Cambio de contraseña opcional
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var changePassResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                        if (!changePassResult.Succeeded)
                        {
                            foreach (var error in changePassResult.Errors)
                                ModelState.AddModelError(string.Empty, error.Description);
                            return View(model);
                        }
                    }

                    TempData["Mensaje"] = $"Asesor {model.Nombre} actualizado correctamente.";
                    TempData["TipoMensaje"] = "success";
                    return RedirectToAction(nameof(Usuarios));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // POST: Eliminar Usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Mensaje"] = "Usuario no encontrado.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Usuarios));
            }

            // No permitir eliminar al usuario actual
            if (user.UserName == User.Identity.Name)
            {
                TempData["Mensaje"] = "No puedes eliminar tu propia cuenta.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Usuarios));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Mensaje"] = "Usuario eliminado exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            else
            {
                TempData["Mensaje"] = "Error al eliminar el usuario.";
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Usuarios));
        }

        // GET: CrearCliente
        public async Task<IActionResult> CrearCliente()
        {
            // Obtener solo los usuarios que tienen el rol de "Asesor"
            var asesores = await _userManager.GetUsersInRoleAsync("Asesor");

            // Pasarlos a la vista para el dropdown
            ViewBag.Asesores = new SelectList(asesores, "Id", "Nombre"); // Usamos tu columna 'Nombre'

            return View();
        }

        // POST: CrearCliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCliente(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.FechaRegistro = DateTime.UtcNow; // Hora UTC para evitar error de Postgres
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Cliente creado exitosamente";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction("Clientes");
            }

            // SI HAY ERROR: Debemos recargar los asesores para que el dropdown no falle
            var asesores = await _userManager.GetUsersInRoleAsync("Asesor");
            ViewBag.Asesores = new SelectList(asesores, "Id", "Nombre");

            return View(cliente);
        }

        // GET: Lista de clientes
        public async Task<IActionResult> Clientes(string? dni, string? nombre, string? estado, string? asesorId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Cargar asesores para el dropdown del filtro
            var asesores = await _userManager.GetUsersInRoleAsync("Asesor");
            ViewBag.Asesores = new SelectList(asesores, "Id", "Nombre");

            // Consulta base
            var query = _context.Clientes.Include(c => c.Asesor).AsQueryable();

            // Filtro por Dni
            if (!string.IsNullOrEmpty(dni))
                query = query.Where(c => c.Dni.Contains(dni));

            // Filtro por Nombre
            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.Nombre.Contains(nombre));

            // Filtro por Estado
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(c => c.Estado == estado);

            // Filtro por Asesor
            if (!string.IsNullOrEmpty(asesorId))
                query = query.Where(c => c.AsesorId == asesorId);

            // Filtro por Rango de Fechas
            if (fechaInicio.HasValue)
                query = query.Where(c => c.FechaRegistro >= fechaInicio.Value.ToUniversalTime());

            if (fechaFin.HasValue)
                query = query.Where(c => c.FechaRegistro <= fechaFin.Value.ToUniversalTime().AddDays(1));

            var model = new ClienteListViewModel
            {
                Clientes = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync(),
                Dni = dni,
                Nombre = nombre,
                Estado = estado,
                AsesorId = asesorId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AsignarAsesor(int clienteId, string asesorId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente != null)
            {
                cliente.AsesorId = string.IsNullOrEmpty(asesorId) ? null : asesorId;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Asesor actualizado correctamente.";
                TempData["TipoMensaje"] = "success";
            }
            return RedirectToAction(nameof(Clientes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarAsesorMasivo(string asesorId, int[] clientesSeleccionados)
        {
            if (clientesSeleccionados == null || clientesSeleccionados.Length == 0)
            {
                TempData["Mensaje"] = "No seleccionaste ningún cliente.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Clientes));
            }

            // Usamos el contexto directamente para asegurar el update
            var clientes = await _context.Clientes
                .Where(c => clientesSeleccionados.Contains(c.Id))
                .ToListAsync();

            foreach (var cliente in clientes)
            {
                // Si el asesorId viene vacío del select, guardamos null
                cliente.AsesorId = string.IsNullOrEmpty(asesorId) ? null : asesorId;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Se actualizaron {clientes.Count} clientes.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error de base de datos: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Clientes));
        }

        // Importar Clientes desde Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarClientes(IFormFile archivoExcel)
        {
            // 1. Validación inicial del archivo
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                TempData["Mensaje"] = "Por favor, seleccione un archivo Excel válido (.xlsx).";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(CrearCliente));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await archivoExcel.CopyToAsync(stream);

                    using (var workbook = new XLWorkbook(stream))
                    {
                        // Leer la primera hoja del Excel
                        var hoja = workbook.Worksheet(1);

                        // Obtenemos todas las filas que tienen datos, saltando la primera (cabecera)
                        var filas = hoja.RangeUsed().RowsUsed().Skip(1);

                        List<Cliente> clientesParaInsertar = new List<Cliente>();
                        int filasVacias = 0;

                        foreach (var fila in filas)
                        {
                            // Extraer datos por número de columna (A=1, B=2, C=3, D=4, E=5)
                            string dni = fila.Cell(1).GetValue<string>().Trim();
                            string nombre = fila.Cell(2).GetValue<string>().Trim();
                            string email = fila.Cell(3).GetValue<string>().Trim();
                            string telefono = fila.Cell(4).GetValue<string>().Trim();
                            string direccion = fila.Cell(5).GetValue<string>().Trim();

                            // Validación mínima: DNI y Nombre no pueden estar vacíos
                            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(nombre))
                            {
                                filasVacias++;
                                continue;
                            }

                            // Creamos el objeto Cliente
                            var nuevoCliente = new Cliente
                            {
                                Dni = dni,
                                Nombre = nombre,
                                Email = email,
                                Telefono = telefono,
                                Direccion = direccion,
                                Estado = "Pendiente",
                                FechaRegistro = DateTime.UtcNow, // Guardamos en UTC para Postgres
                                AsesorId = null
                            };

                            clientesParaInsertar.Add(nuevoCliente);
                        }

                        if (clientesParaInsertar.Count > 0)
                        {
                            // Guardar todos los clientes de golpe en la base de datos
                            _context.Clientes.AddRange(clientesParaInsertar);
                            await _context.SaveChangesAsync();

                            TempData["Mensaje"] = $"Se han importado {clientesParaInsertar.Count} clientes correctamente.";
                            TempData["TipoMensaje"] = "success";
                        }
                        else
                        {
                            TempData["Mensaje"] = "El archivo no contenía datos válidos o estaba vacío.";
                            TempData["TipoMensaje"] = "warning";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al importar clientes desde Excel");
                TempData["Mensaje"] = "Ocurrió un error al procesar el archivo. Asegúrese de usar el formato correcto.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(CrearCliente));
            }

            return RedirectToAction(nameof(Clientes));
        }

        // Generar Reportes
        public async Task<IActionResult> ExportarClientes(string? dni, string? nombre, string? estado, string? asesorId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Clientes.Include(c => c.Asesor).AsQueryable();

            // Aplicamos los mismos filtros que en la vista
            if (!string.IsNullOrEmpty(dni))
                query = query.Where(c => c.Dni.Contains(dni));

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.Nombre.ToLower().Contains(nombre.ToLower()));

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(c => c.Estado == estado);

            if (!string.IsNullOrEmpty(asesorId))
                query = query.Where(c => c.AsesorId == asesorId);

            if (fechaInicio.HasValue)
                query = query.Where(c => c.FechaRegistro >= fechaInicio.Value.ToUniversalTime());

            if (fechaFin.HasValue)
                query = query.Where(c => c.FechaRegistro <= fechaFin.Value.ToUniversalTime().AddDays(1));

            var clientes = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync();

            // Generación del Excel con ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte de Clientes");
                var currentRow = 1;

                // Cabeceras
                worksheet.Cell(currentRow, 1).Value = "Fecha Registro";
                worksheet.Cell(currentRow, 2).Value = "DNI";
                worksheet.Cell(currentRow, 3).Value = "Nombre Cliente";
                worksheet.Cell(currentRow, 4).Value = "Email";
                worksheet.Cell(currentRow, 5).Value = "Teléfono";
                worksheet.Cell(currentRow, 6).Value = "Dirección";
                worksheet.Cell(currentRow, 7).Value = "Estado";
                worksheet.Cell(currentRow, 8).Value = "Asesor Asignado";

                // Estilo de cabecera
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.Black;
                headerRow.Style.Font.FontColor = XLColor.White;

                // Datos
                var zonaPeru = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                foreach (var cliente in clientes)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = TimeZoneInfo.ConvertTimeFromUtc(cliente.FechaRegistro, zonaPeru).ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(currentRow, 2).Value = cliente.Dni;
                    worksheet.Cell(currentRow, 3).Value = cliente.Nombre;
                    worksheet.Cell(currentRow, 4).Value = cliente.Email;
                    worksheet.Cell(currentRow, 5).Value = cliente.Telefono;
                    worksheet.Cell(currentRow, 6).Value = cliente.Direccion;
                    worksheet.Cell(currentRow, 7).Value = cliente.Estado;
                    worksheet.Cell(currentRow, 8).Value = cliente.Asesor?.Nombre ?? "Sin asignar";
                }

                worksheet.Columns().AdjustToContents(); // Ajustar ancho de columnas automáticamente

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"Reporte_Clientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // POST: Eliminar Cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                TempData["Mensaje"] = "El cliente no existe o ya fue eliminado.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Clientes));
            }

            try
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"El cliente {cliente.Nombre} ha sido eliminado correctamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el cliente");
                TempData["Mensaje"] = "Hubo un error al intentar eliminar el cliente.";
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Clientes));
        }
    }
}