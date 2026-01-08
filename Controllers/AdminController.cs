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

        // --- DASHBOARD ---
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsuarios = await _userManager.Users.CountAsync(),
                TotalRoles = await _roleManager.Roles.CountAsync(),
                TotalClientes = await _context.Clientes.CountAsync(),
                AsesoresEnLinea = await _context.Users.CountAsync(u => u.EstaConectado),
                UsuarioActual = User.Identity.Name
            };
            return View(model);
        }

        // --- GESTIÓN DE ASESORES (USUARIOS) ---

        public IActionResult Usuarios() => View(_userManager.Users.ToList());

        public IActionResult CrearAsesor() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAsesor(CrearAsesorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuarioExistente = await _userManager.FindByNameAsync(model.Dni);
                if (usuarioExistente != null)
                {
                    ModelState.AddModelError("Dni", "Ya existe un usuario con este DNI.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Dni,
                    Email = $"{model.Dni}@accob.com",
                    EmailConfirmed = true,
                    Nombre = model.Nombre,
                    Dni = model.Dni,
                    Celular = model.Celular,
                    PhoneNumber = model.Celular
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Asesor");
                    TempData["Mensaje"] = $"Asesor {model.Nombre} creado exitosamente.";
                    TempData["TipoMensaje"] = "success";
                    return RedirectToAction("Usuarios");
                }
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        public async Task<IActionResult> EditarAsesor(string id)
        {
            if (id == null) return NotFound();
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();
            return View(new EditarAsesorViewModel { Id = usuario.Id, Nombre = usuario.Nombre, Dni = usuario.Dni, Celular = usuario.Celular });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAsesor(EditarAsesorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                var conMismoDni = await _userManager.FindByNameAsync(model.Dni);
                if (conMismoDni != null && conMismoDni.Id != user.Id)
                {
                    ModelState.AddModelError("Dni", "DNI ya registrado por otro usuario.");
                    return View(model);
                }

                user.Nombre = model.Nombre;
                user.Dni = model.Dni;
                user.UserName = model.Dni;
                user.Celular = model.Celular;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    }
                    TempData["Mensaje"] = "Asesor actualizado.";
                    TempData["TipoMensaje"] = "success";
                    return RedirectToAction(nameof(Usuarios));
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (user.UserName == User.Identity.Name)
            {
                TempData["Mensaje"] = "No puedes eliminar tu cuenta.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Usuarios));
            }
            await _userManager.DeleteAsync(user);
            TempData["Mensaje"] = "Usuario eliminado.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Usuarios));
        }


        // --- GESTIÓN DE CLIENTES ---

        public async Task<IActionResult> Clientes(string? nombre, string? estado, string? asesorId, DateTime? fechaInicio, DateTime? fechaFin, string? provincia, string? distrito)
        {
            var asesores = await _userManager.GetUsersInRoleAsync("Asesor");
            ViewBag.Asesores = new SelectList(asesores, "Id", "Nombre");

            var query = _context.Clientes
                .Include(c => c.Asesor)
                .Include(c => c.Ventas)
                .Include(c => c.Llamadas).ThenInclude(l => l.Asesor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
            {
                string n = nombre.ToLower().Trim();
                query = query.Where(c => c.Nombre.ToLower().Contains(n) || c.Apellido.ToLower().Contains(n) || c.Dni.Contains(n));
            }
            if (!string.IsNullOrEmpty(estado)) query = query.Where(c => c.Estado == estado);
            if (!string.IsNullOrEmpty(asesorId)) query = (asesorId == "sin_asignar") ? query.Where(c => c.AsesorId == null) : query.Where(c => c.AsesorId == asesorId);
            if (!string.IsNullOrEmpty(provincia)) query = query.Where(c => c.Provincia.Contains(provincia));
            if (!string.IsNullOrEmpty(distrito)) query = query.Where(c => c.Distrito.Contains(distrito));

            if (fechaInicio.HasValue) query = query.Where(c => c.FechaRegistro >= fechaInicio.Value.ToUniversalTime());
            if (fechaFin.HasValue) query = query.Where(c => c.FechaRegistro <= fechaFin.Value.ToUniversalTime().AddDays(1));

            var model = new ClienteListViewModel
            {
                Clientes = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync(),
                Nombre = nombre,
                Estado = estado,
                AsesorId = asesorId,
                Provincia = provincia,
                Distrito = distrito,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            return View(model);
        }

        public async Task<IActionResult> CrearCliente()
        {
            var asesores = await _userManager.GetUsersInRoleAsync("Asesor");
            ViewBag.Asesores = new SelectList(asesores, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCliente(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.Telefono = LimpiarTelefono(cliente.Telefono);
                cliente.NumRef1 = LimpiarTelefono(cliente.NumRef1);
                cliente.NumRef2 = LimpiarTelefono(cliente.NumRef2);
                cliente.FechaRegistro = DateTime.UtcNow;
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cliente registrado.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Clientes));
            }
            var asesores = await _userManager.GetUsersInRoleAsync("Asesor");
            ViewBag.Asesores = new SelectList(asesores, "Id", "Nombre");
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarAsesor(int clienteId, string? asesorId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente != null)
            {
                cliente.AsesorId = string.IsNullOrEmpty(asesorId) ? null : asesorId;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Asignación actualizada.";
                TempData["TipoMensaje"] = "success";
            }
            return RedirectToAction(nameof(Clientes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarAsesorMasivo(string asesorId, int[] clientesSeleccionados)
        {
            if (clientesSeleccionados == null || clientesSeleccionados.Length == 0) return RedirectToAction(nameof(Clientes));
            var clientes = await _context.Clientes.Where(c => clientesSeleccionados.Contains(c.Id)).ToListAsync();
            foreach (var c in clientes) c.AsesorId = string.IsNullOrEmpty(asesorId) ? null : asesorId;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = $"Se asignaron {clientes.Count} clientes.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Clientes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cliente eliminado.";
                TempData["TipoMensaje"] = "success";
            }
            return RedirectToAction(nameof(Clientes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarClientesMasivo(int[] clientesSeleccionados)
        {
            if (clientesSeleccionados == null || clientesSeleccionados.Length == 0) return RedirectToAction(nameof(Clientes));
            try
            {
                var clientes = await _context.Clientes.Where(c => clientesSeleccionados.Contains(c.Id)).ToListAsync();
                _context.Clientes.RemoveRange(clientes);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Se eliminaron {clientes.Count} registros.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception)
            {
                TempData["Mensaje"] = "Error al eliminar masivamente.";
                TempData["TipoMensaje"] = "danger";
            }
            return RedirectToAction(nameof(Clientes));
        }

        // --- IMPORTACIÓN Y EXPORTACIÓN ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarClientes(IFormFile archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0) return RedirectToAction(nameof(CrearCliente));
            try
            {
                var dnisExistentes = await _context.Clientes.Select(c => c.Dni).ToHashSetAsync();
                var dnisEnArchivo = new HashSet<string>();

                using var stream = new MemoryStream();
                await archivoExcel.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var hoja = workbook.Worksheet(1);
                var primeraFila = hoja.Row(1);

                int colDni = 0, colNom = 0, colApe = 0, colEmail = 0, colTel = 0, colDep = 0, colProv = 0, colDist = 0, colDir = 0, colRef1 = 0, colRef2 = 0;
                for (int i = 1; i <= hoja.LastColumnUsed().ColumnNumber(); i++)
                {
                    string h = primeraFila.Cell(i).GetValue<string>().Trim().ToLower();
                    if (h == "dni") colDni = i;
                    else if (h.Contains("nombre")) colNom = i;
                    else if (h.Contains("apellido")) colApe = i;
                    else if (h.Contains("email") || h.Contains("correo")) colEmail = i;
                    else if (h.Contains("tel") || h.Contains("celular")) colTel = i;
                    else if (h.Contains("dep")) colDep = i;
                    else if (h.Contains("prov")) colProv = i;
                    else if (h.Contains("dist")) colDist = i;
                    else if (h.Contains("dir")) colDir = i;
                    else if (h.Contains("ref1") || h.Contains("referencia")) colRef1 = i;
                    else if (h.Contains("ref2")) colRef2 = i;
                }

                var filas = hoja.RangeUsed().RowsUsed().Skip(1);
                List<Cliente> listParaInsertar = new List<Cliente>();

                foreach (var fila in filas)
                {
                    string Leer(int col)
                    {
                        if (col <= 0) return "";
                        string val = fila.Cell(col).GetValue<string>().Trim();
                        return (val == "\\N" || string.IsNullOrEmpty(val)) ? "" : val;
                    }
                    string dni = Leer(colDni);
                    string nom = Leer(colNom);
                    if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(nom) || dnisExistentes.Contains(dni) || dnisEnArchivo.Contains(dni)) continue;
                    dnisEnArchivo.Add(dni);

                    listParaInsertar.Add(new Cliente
                    {
                        Dni = dni,
                        Nombre = nom,
                        Apellido = Leer(colApe),
                        Telefono = LimpiarTelefono(Leer(colTel)),
                        Email = !string.IsNullOrEmpty(Leer(colEmail)) ? Leer(colEmail) : "sin@correo.com",
                        Departamento = !string.IsNullOrEmpty(Leer(colDep)) ? Leer(colDep) : "LIMA",
                        Provincia = !string.IsNullOrEmpty(Leer(colProv)) ? Leer(colProv) : "LIMA",
                        Distrito = Leer(colDist),
                        Direccion = Leer(colDir),
                        NumRef1 = LimpiarTelefono(Leer(colRef1)),
                        NumRef2 = LimpiarTelefono(Leer(colRef2)),
                        Estado = "Pendiente",
                        FechaRegistro = DateTime.UtcNow
                    });
                }

                if (listParaInsertar.Count > 0)
                {
                    foreach (var chunk in listParaInsertar.Chunk(1000))
                    {
                        _context.Clientes.AddRange(chunk);
                        await _context.SaveChangesAsync();
                    }
                    TempData["Mensaje"] = $"Importados {listParaInsertar.Count} clientes.";
                    TempData["TipoMensaje"] = "success";
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error importar"); TempData["Mensaje"] = "Error al procesar."; TempData["TipoMensaje"] = "danger"; }
            return RedirectToAction(nameof(Clientes));
        }

        public async Task<IActionResult> ExportarClientes(string? nombre, string? estado, string? asesorId, DateTime? fechaInicio, DateTime? fechaFin, string? provincia, string? distrito)
        {
            var query = _context.Clientes
                .Include(c => c.Asesor)
                .Include(c => c.Ventas)

                .AsQueryable();
            if (!string.IsNullOrEmpty(nombre))
            {
                string n = nombre.ToLower().Trim();
                query = query.Where(c => c.Nombre.ToLower().Contains(n) || c.Apellido.ToLower().Contains(n) || c.Dni.Contains(n));
            }
            if (!string.IsNullOrEmpty(estado)) query = query.Where(c => c.Estado == estado);
            if (!string.IsNullOrEmpty(provincia)) query = query.Where(c => c.Provincia.Contains(provincia));
            if (!string.IsNullOrEmpty(distrito)) query = query.Where(c => c.Distrito.Contains(distrito));

            var clientes = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte");
            string[] headers = { "Fecha Reg", "DNI", "Cliente", "Teléfono", "Estado", "Asesor", "Zona", "Plan", "Distrito", "Provincia" };
            for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];

            int row = 2;
            foreach (var c in clientes)
            {
                var dv = c.Ventas?.OrderByDescending(v => v.FechaVenta).FirstOrDefault();
                ws.Cell(row, 1).Value = c.FechaRegistro.ToLocalTime().ToString("g");
                ws.Cell(row, 2).Value = c.Dni;
                ws.Cell(row, 3).Value = c.NombreCompleto;
                ws.Cell(row, 4).Value = c.Telefono;
                ws.Cell(row, 5).Value = c.Estado;
                ws.Cell(row, 6).Value = c.Asesor?.Nombre ?? "-";
                if (dv != null) { ws.Cell(row, 7).Value = dv.ZonaNombre; ws.Cell(row, 8).Value = dv.PlanNombre; }
                ws.Cell(row, 9).Value = c.Distrito; ws.Cell(row, 10).Value = c.Provincia;
                row++;
            }
            ws.Columns().AdjustToContents();
            using var s = new MemoryStream();
            workbook.SaveAs(s);
            return File(s.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_ACCOB_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // --- UTILITARIOS ---
        private string? LimpiarTelefono(string? tel)
        {
            if (string.IsNullOrEmpty(tel)) return tel;
            string t = tel.Trim();
            if (t.StartsWith("51") && t.Length > 9) return t.Substring(2);
            return t;
        }
    }
}