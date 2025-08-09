using API.Models.Entities;
using API.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace API.Models.Services.Infrastructure.Middlewares
{
    public class CheckUserNotDeletedMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserNotDeletedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if(context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = userManager.GetUserId(context.User);
                if(string.IsNullOrEmpty(userId)) return;
                var user = await userManager.FindByIdAsync(userId);

                if (user != null && user.Status == ApplicationUserStatus.Inattivo) {

                    if (context.Request.Cookies.ContainsKey("jwtToken"))
                        context.Response.Cookies.Delete("jwtToken");
                    
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Account disabilitato o eliminato, non è possibile accedere");
                    return; // Blocca richiesta HTTP 
                }
            }

            await _next(context);
        }
    }
}
