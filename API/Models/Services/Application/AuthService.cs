using System.Net;
using API.Models.DTO;
using API.Models.Entities;
using API.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Application
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenBlacklist _tokenBlacklist;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenBlacklist tokenBlacklist,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender,
            ApplicationDbContext dbContext,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _tokenBlacklist = tokenBlacklist;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterRequestDto request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Nome = request.Nome,
                Cognome = request.Cognome
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            var cliente = new Cliente
            {
                Nome = request.Nome,
                Cognome = request.Cognome,
                Email = request.Email,
                UserId = user.Id
            };

            _dbContext.Clienti.Add(cliente);
            await _dbContext.SaveChangesAsync();

            if (result.Succeeded)
            {
                // Verifica esistenza ruolo e creazione se necessario
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                // Aggiungi ruolo all'utente
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        Console.WriteLine($"Errore ruolo: {error.Description}");
                }

                // verifica email tramite token
                // string? token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // string? encodedToken = WebUtility.UrlEncode(token);
                // string? linkConferma = $"{_configuration["ApiUrl"]}api/auth/confirmEmail?userid={user.Id}&token={encodedToken}";

                // // Invia email di conferma 
                // await _emailSender.SendEmailAsync(user.Email, "Conferma la tua registrazione",
                //     $@"
                //     <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; border-radius: 10px;"">
                //         <h2 style=""color: #333;"">Benvenuto, {user.Nome}!</h2>
                //         <p style=""font-size: 16px; color: #555;"">
                //             Grazie per esserti registrato. Per completare la registrazione, ti chiediamo di confermare il tuo indirizzo email.
                //         </p>
                //         <div style=""text-align: center; margin: 30px 0;"">
                //             <a href=""{linkConferma}"" style=""background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; font-size: 16px; border-radius: 5px;"">
                //                 Conferma il tuo account
                //             </a>
                //         </div>
                //         <p style=""font-size: 14px; color: #888;"">
                //             Se il pulsante non funziona, copia e incolla questo link nel tuo browser:<br/>
                //             <a href=""{linkConferma}"" style=""color: #4CAF50;"">{linkConferma}</a>
                //         </p>
                //         <hr style=""margin: 30px 0;""/>
                //         <p style=""font-size: 12px; color: #aaa;"">
                //             Se non hai richiesto questa registrazione, puoi ignorare questa email.
                //         </p>
                //     </div>
                //     ");

            }

            return result;
        }

        public async Task<ApplicationUser> FindUserAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> UpdateAccountAsync(UpdateAccountRequestDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null) return false;

                // Aggiornamento dell'utente
                user.Nome = dto.Nome;
                user.Cognome = dto.Cognome;
                user.Email = dto.Email;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) return false;

                var cliente = await _dbContext.Clienti.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (cliente == null) return false;

                // Aggiornamento indirizzo
                cliente.Indirizzo.Via = dto.Indirizzo_Via;
                cliente.Indirizzo.CAP = dto.Indirizzo_CAP;
                cliente.Indirizzo.Citta = dto.Indirizzo_Citta;
                cliente.Indirizzo.HouseNumber = dto.Indirizzo_HouseNumber;

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<bool> UpdatePasswordAsync(UpdatePasswordRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) return false; // Account non trovato

            if (!await _userManager.CheckPasswordAsync(user, dto.PasswordCorrente))
                return false;

            var result = await _userManager.ChangePasswordAsync(user, dto.PasswordCorrente, dto.NuovaPassword);
            return true;
        }

        public async Task<ApplicationUserDto> GetAppUserAsync(ApplicationUser user)
        {
            bool is2FaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            return new ApplicationUserDto
            {
                UserId = user.Id,
                Nome = user.Nome,
                Cognome = user.Cognome,
                Email = user.Email,
                Is2FAEnabled = is2FaEnabled,

            };
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            user.ChangeStatus(Models.Enums.ApplicationUserStatus.Eliminato);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDtoRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"http://localhost:5100/Auth/ResetPasswordPage?token={Uri.EscapeDataString(token)}&userid={user.Id}";

            await _emailSender.SendEmailAsync(request.Email, "Recupero Password",
                $@"
                <html>
                    <body style='margin:0; padding:0; font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif; background-color: #f0f4f8;'>
                        <div style='max-width: 600px; margin: 20px auto; padding: 40px 30px; background: linear-gradient(135deg, #ffffff 0%, #f8faff 100%); border-radius: 25px; box-shadow: 0 10px 40px rgba(0,0,0,0.08); border: 1px solid #e3eaf3;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <div style='display: inline-block; background: #1d4ed8; width: 60px; height: 60px; border-radius: 50%; padding: 15px; margin-bottom: 25px;'>
                                    <svg style='width: 30px; height: 30px; fill: white;' viewBox='0 0 24 24' aria-hidden='true'>
                                        <path d='M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1 1.71 0 3.1 1.39 3.1 3.1v2z'/>
                                    </svg>
                                </div>
                                <h1 style='margin:0 0 15px 0; font-size: 28px; color: #1f2937; letter-spacing: -0.5px;'>
                                    Reimposta la tua password
                                </h1>
                                <p style='margin:0; color: #6b7280; font-size: 15px; line-height: 1.6;'>
                                    Clicca sul pulsante qui sotto per creare una nuova password
                                </p>
                            </div>

                            <div style='text-align: center; margin: 35px 0;'>
                                <a href='{resetLink}' style='display: inline-block; background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%); color: white; padding: 14px 35px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 15px; letter-spacing: 0.5px; transition: all 0.3s ease; box-shadow: 0 4px 15px rgba(29,78,216,0.25); position: relative; overflow: hidden;'>
                                    <span style='position: relative; z-index: 2;'>Reimposta Password</span>
                                    <div style='position: absolute; top: -50%; left: -50%; width: 200%; height: 200%; background: linear-gradient(45deg, transparent 30%, rgba(255,255,255,0.15) 50%, transparent 70%); animation: shine 3s infinite; z-index: 1;'/>
                                </a>
                            </div>

                            <div style='text-align: center; padding: 25px 20px; background-color: #f8fafc; border-radius: 12px; margin-top: 35px; border: 1px solid #e2e8f0;'>
                                <p style='margin:0; color: #64748b; font-size: 13px; line-height: 1.6;'>
                                    Se non hai richiesto il reset della password, puoi ignorare questa email.<br>
                                    Il link scadrà tra 24 ore. Per sicurezza, non condividere questo link con nessuno.
                                </p>
                            </div>

                            <style>
                                @keyframes shine {{
                                    0% {{ transform: translateX(-100%) rotate(45deg); }}
                                    100% {{ transform: translateX(100%) rotate(45deg); }}
                                }}
                            </style>
                        </div>
                    </body>
                </html>"
            );

            return true;

        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NuovaPassword);
            return result.Succeeded ? true : false;
        }
    }
}
