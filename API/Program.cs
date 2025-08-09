using API.Models.Requests.Validators;
using API.Models.Services.Infrastructure;
using API.Models.Services.Infrastructure.Hubs;
using API.Models.Services.Infrastructure.Middlewares;
using API.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://+:80");

/****************************************
 *          Services Configuration      *
 ****************************************/

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
//QuestPDF.Settings.EnableDebugging = true;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configura Swagger 
builder.Services.AddSwagger();
// Configura CORS 
builder.Services.AddCorsWithDefaultValues();

// configura Database 
builder.Services.AddDbContextDefault(builder.Configuration);

// configura Identity 
builder.Services.AddIdentityDefault();

// configura autenticazione
builder.Services.AddJwtAuthentication(builder.Configuration);

// configura autorizzazione 
builder.Services.AddAuthorization();

// configura HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Application Services
builder.Services.AddApplicationServices(builder.Configuration);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();

/****************************************
 *          Middleware Pipeline         *
 ****************************************/
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // Configura la UI di Swagger
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v2");
        c.RoutePrefix = string.Empty;  // Renderizza Swagger UI alla root del progetto
    });

    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/error");
}

// Questo middleware ( si trova in models >> services >> infrastructure >> middlewares) controlla se l'ip del client è bannato
app.UseMiddleware<BannedIpMiddleware>();

app.UseStaticFiles();

app.UseRouting();

// CORS va tra UseRouting e UseAuthentication
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Esegui la migrazione del DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Middleware custom dopo autenticazione
app.UseMiddleware<MaintenanceMiddleware>();
app.UseMiddleware<CheckUserNotDeletedMiddleware>();

// Endpoint mapping (inclusi Controller e SignalR)
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ProdottoHub>("/prodottoHub");
});

// Endpoint di default su Swagger
app.MapGet("/", () => Results.LocalRedirect("/swagger"));

app.Run();
