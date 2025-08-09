using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Infrastructure.Middlewares;

public class BannedIpMiddleware
{
    private readonly RequestDelegate _next;

    public BannedIpMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ApplicationDbContext dbcontext)
    {
        var ip = GetClienteIp(context);

        if(!string.IsNullOrEmpty(ip)){
            var isBannedIp = await dbcontext.BannedIps.AnyAsync(bannedIp => bannedIp.Ip == ip);

            if(isBannedIp)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Accesso negato, il tuo account è bannato");
                return;
            }
        }

        await _next(context);
    }

    private string GetClienteIp(HttpContext context) {

        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if(!string.IsNullOrEmpty(ip))
        {
            ip = ip.Split(", ")[0].Trim();
        }
        else ip = context.Connection.RemoteIpAddress?.ToString();

        return ip ?? "";
    }
}
