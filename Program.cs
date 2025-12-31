using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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

app.Run();
