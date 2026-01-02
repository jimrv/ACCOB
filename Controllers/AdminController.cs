using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        // GET: Configuración
        public IActionResult Configuracion()
        {
            return View();
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
        public IActionResult Clientes()
        {
            // Usamos .Include(c => c.Asesor) para traer los datos del asesor asignado
            var clientes = _context.Clientes.Include(c => c.Asesor).ToList();
            return View(clientes);
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