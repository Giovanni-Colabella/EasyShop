namespace API.Models.Services.Infrastructure.Middlewares
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration config;
        public MaintenanceMiddleware(RequestDelegate next, IConfiguration config)
        {
            this.next = next;
            this.config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                bool manutenzioneAttiva = config.GetValue<bool>("Maintenance");

                if (manutenzioneAttiva)
                {
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    await context.Response.WriteAsync("Il sito è in manutenzione. Richiesta HTTP bloccata");
                    return;
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Errore generico: {ex.Message}");
                return;
            }

            await next(context);
        }
    }
}
