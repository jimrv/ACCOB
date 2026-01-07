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

        // --- MÉTODOS DE ASESORES ---
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

        public IActionResult Usuarios() => View(_userManager.Users.ToList());

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

                var usuarioConMismoDni = await _userManager.FindByNameAsync(model.Dni);
                if (usuarioConMismoDni != null && usuarioConMismoDni.Id != user.Id)
                {
                    ModelState.AddModelError("Dni", "Este DNI ya está registrado.");
                    return View(model);
                }

                user.Nombre = model.Nombre; user.Dni = model.Dni; user.UserName = model.Dni;
                user.Celular = model.Celular; user.PhoneNumber = model.Celular;

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

        // --- GESTIÓN DE CLIENTES ---
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
                TempData["Mensaje"] = "Cliente creado.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction("Clientes");
            }
            return View(cliente);
        }

        public async Task<IActionResult> Clientes(string? dni, string? nombre, string? estado, string? asesorId, DateTime? fechaInicio, DateTime? fechaFin, string? provincia, string? distrito)
        {
            var asesores = await _userManager.GetUsersInRoleAsync("Asesor");
            ViewBag.Asesores = new SelectList(asesores, "Id", "Nombre");
            var query = _context.Clientes.Include(c => c.Asesor).Include(c => c.Ventas).AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
            {
                string n = nombre.ToLower().Trim();
                query = query.Where(c => c.Nombre.ToLower().Contains(n) || c.Apellido.ToLower().Contains(n) || c.Dni.Contains(n));
            }
            if (!string.IsNullOrEmpty(estado)) query = query.Where(c => c.Estado == estado);
            if (!string.IsNullOrEmpty(provincia)) query = query.Where(c => c.Provincia.Contains(provincia));
            if (!string.IsNullOrEmpty(distrito)) query = query.Where(c => c.Distrito.Contains(distrito));

            return View(new ClienteListViewModel
            {
                Clientes = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync(),
                Nombre = nombre,
                Estado = estado,
                Provincia = provincia,
                Distrito = distrito
            });
        }

        // --- IMPORTACIÓN MASIVA OPTIMIZADA (85K) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarClientes(IFormFile archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0) return RedirectToAction("CrearCliente");

            try
            {
                // 1. Cargar DNI existentes de la base de datos para omitirlos instantáneamente
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
                List<Cliente> clientesParaInsertar = new List<Cliente>();

                foreach (var fila in filas)
                {
                    string Leer(int col)
                    {
                        if (col <= 0) return "";
                        string val = fila.Cell(col).GetValue<string>().Trim();
                        return (val == "\\N" || string.IsNullOrEmpty(val)) ? "" : val;
                    }

                    string dni = Leer(colDni);
                    string nombre = Leer(colNom);

                    // OMITIR SI: DNI vacío, ya existe en BD, o está repetido en el mismo Excel
                    if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(nombre) ||
                        dnisExistentes.Contains(dni) || dnisEnArchivo.Contains(dni)) continue;

                    dnisEnArchivo.Add(dni);

                    clientesParaInsertar.Add(new Cliente
                    {
                        Dni = dni,
                        Nombre = nombre,
                        Apellido = Leer(colApe),
                        Email = !string.IsNullOrEmpty(Leer(colEmail)) ? Leer(colEmail) : "sin@correo.com",
                        Telefono = LimpiarTelefono(Leer(colTel)),
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

                if (clientesParaInsertar.Count > 0)
                {
                    // Guardar en bloques de 1000 para evitar errores de memoria o timeout
                    foreach (var chunk in clientesParaInsertar.Chunk(1000))
                    {
                        _context.Clientes.AddRange(chunk);
                        await _context.SaveChangesAsync();
                    }
                    TempData["Mensaje"] = $"Éxito: Se importaron {clientesParaInsertar.Count} nuevos clientes.";
                    TempData["TipoMensaje"] = "success";
                }
                else
                {
                    TempData["Mensaje"] = "No se encontraron clientes nuevos para importar.";
                    TempData["TipoMensaje"] = "warning";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importar");
                TempData["Mensaje"] = "Error técnico: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction("CrearCliente");
            }
            return RedirectToAction(nameof(Clientes));
        }

        // --- UTILITARIOS ---
        private string? LimpiarTelefono(string? tel)
        {
            if (string.IsNullOrEmpty(tel)) return tel;
            string t = tel.Trim();
            if (t.StartsWith("51") && t.Length > 9) return t.Substring(2);
            return t;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null) { _context.Clientes.Remove(cliente); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Clientes));
        }
    }
}