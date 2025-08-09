using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;

using API.Models.Entities;
using API.Models.Options;
using API.Models.Services.Application;
using API.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API.Models.Services.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            // Definizione della documentazione
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API",
                Version = "v2",
                Description = "API per la gestione di clienti e ordini.",
                Contact = new OpenApiContact
                {
                    Name = "Giovanni Colabella",
                    Email = "giovannicolabell@gmail.com"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Inserisci il token JWT",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

        });

        return services;
    }


    public static IServiceCollection AddCorsWithDefaultValues(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
                builder.WithOrigins("http://localhost:5100")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true));
        });
        return services;
    }
    public static IServiceCollection AddDbContextDefault(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
        return services;
    }
    public static IServiceCollection AddIdentityDefault(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        services.Configure<DataProtectionTokenProviderOptions>(options => {
            options.TokenLifespan = TimeSpan.FromHours(2); // il token ora ha scadenza di due ore, di default 24
        });

        services.Configure<IdentityOptions>(options => {
            options.SignIn.RequireConfirmedEmail = false;
        });
        return services;
    }
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key,
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    var token = ctx.Request.Cookies["jwtToken"];

                    if (!string.IsNullOrEmpty(token))
                        ctx.Request.Headers["Authorization"] = $"Bearer {token}";

                    return Task.CompletedTask;
                },

                OnTokenValidated = async ctx =>
                {
                    // Parsing esplicito del token da Authorization header
                    var authHeader = ctx.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                    var tokenString = authHeader?.Split(" ").Last();

                    if (string.IsNullOrEmpty(tokenString))
                    {
                        ctx.Fail("Token mancante");
                        return;
                    }

                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var token = handler.ReadJwtToken(tokenString); 

                        var blackListService = ctx.HttpContext.RequestServices.GetRequiredService<ITokenBlacklist>();

                        if (await blackListService.IsRevoked(token.RawData))
                        {
                            ctx.Fail("Token revocato");
                            return;
                        }

                        ctx.Success();
                    }
                    catch (Exception ex)
                    {
                        ctx.Fail($"Token non valido: {ex.Message}");
                    }
                }

            };

        });

        return services;
    }
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IClienteService, EfCoreClienteService>();
        services.AddScoped<IOrdineService, EfCoreOrdineService>();
        services.AddScoped<IProdottoService, EfCoreProdottoService>();
        services.AddScoped<IImagePersister, ImageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<ITokenBlacklist, TokenBlacklist>();
        services.AddScoped<IUtenteBloccatoService, BanUserByIpService>();
        services.AddScoped<IRolesService, EfCoreRolesService>();
        services.AddScoped<ICarrelloService, EfCoreCarrelloService>();
        services.AddScoped<ImageProfilePictureService>();
        services.AddScoped<IProfilePictureService, EfCoreProfilePictureService>();
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddHttpClient();
        services.AddScoped<PayPalService>();
        services.AddSignalR();
        services.AddScoped<ExcelImportService>();
        services.AddSingleton<IPdfService, PdfGeneratorService>();

        services.Configure<SmtpSettings>(config.GetSection("SmtpSettings"));
        services.Configure<PayPalSettings>(config.GetSection("PayPal"));
        

        return services;
    }


}
