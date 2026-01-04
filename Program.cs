using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using ACCOB.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // LOGIN SOLO CON USUARIO
    options.User.RequireUniqueEmail = false;

    // ⭐ CONFIGURACIÓN DE CONTRASEÑAS PERSONALIZADA
    options.Password.RequireDigit = true;              // Sí requiere números
    options.Password.RequiredLength = 6;                // Longitud mínima: 6 caracteres
    options.Password.RequireNonAlphanumeric = false;    // NO requiere caracteres especiales
    options.Password.RequireUppercase = false;          // NO requiere mayúsculas obligatorias
    options.Password.RequireLowercase = false;          // NO requiere minúsculas obligatorias
    options.Password.RequiredUniqueChars = 1;           // Solo 1 carácter único mínimo

})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews(options =>
{
    // Crear una política que exija autenticación
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Aplicar la política a todos los controladores de forma global
    options.Filters.Add(new AuthorizeFilter(policy));
});


builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapHub<PresenceHub>("/presenceHub");

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

//generar admin y roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Asesor" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // 🔹 ADMIN
    var adminUser = await userManager.FindByNameAsync("admin");

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            NombreUsuario = "admin",
            Email = "accobsac@gmail.com",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, "Admin123*");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Resetear estados de conexión al iniciar el servidor
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var usuariosConectados = context.Users.Where(u => u.EstaConectado).ToList();
    foreach (var u in usuariosConectados)
    {
        u.EstaConectado = false;
    }
    context.SaveChanges();
}

app.MapGet("/", context =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Identity/Account/Login");
    }
    else
    {
        context.Response.Redirect("/Home/Index");
    }
    return Task.CompletedTask;
});

// Sembrar Planes Win y Zonas
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedWinData(context); // Esto llena la base de datos
}

app.Run();
