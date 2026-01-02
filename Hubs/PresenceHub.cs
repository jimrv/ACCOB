using Microsoft.AspNetCore.SignalR;
using ACCOB.Data;

public class PresenceHub : Hub
{
    private readonly ApplicationDbContext _context;

    public PresenceHub(ApplicationDbContext context) => _context = context;

    public override async Task OnConnectedAsync()
    {
        var user = _context.Users.FirstOrDefault(u => u.UserName == Context.User.Identity.Name);
        if (user != null)
        {
            user.EstaConectado = true;
            await _context.SaveChangesAsync();

            // Contamos cuántos están en línea ahora
            var totalEnLinea = _context.Users.Count(u => u.EstaConectado);

            // Avisamos a todos los clientes el nuevo conteo
            await Clients.All.SendAsync("UpdateOnlineCount", totalEnLinea);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserName == Context.User.Identity.Name);
        if (user != null)
        {
            user.EstaConectado = false;
            await _context.SaveChangesAsync();

            var totalEnLinea = _context.Users.Count(u => u.EstaConectado);
            await Clients.All.SendAsync("UpdateOnlineCount", totalEnLinea);
        }
        await base.OnDisconnectedAsync(exception);
    }
}