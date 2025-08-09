using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Models;
using API.Models.DTO;
using API.Models.Entities;
using API.Models.Services.Application;
using API.Services;
using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<UpdateAccountRequestDto> _accountValidator;
    private readonly IValidator<UpdatePasswordRequestDto> _passwordValidator;
    private readonly IConfiguration _configuration;
    private readonly ITokenBlacklist _tokenBlacklist;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<UpdateAccountRequestDto> accountValidator,
        IValidator<UpdatePasswordRequestDto> passwordValidator,
        IConfiguration configuration,
        IValidator<LoginRequestDto> loginValidator,
        ITokenBlacklist tokenBlacklist,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _accountValidator = accountValidator;
        _passwordValidator = passwordValidator;
        _configuration = configuration;
        _tokenBlacklist = tokenBlacklist;
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Errors = errorMessages });
        }
        var result = await _authService.RegisterAsync(request);

        if (!result.Succeeded)
        {
            var identityErrors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { Errors = identityErrors });
        }


        return Ok("Utente registrato!");
    }



    private string GetClientIp()
    {
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrEmpty(ip))
        {
            ip = ip.Split(',')[0].Trim();
        }
        else
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        return ip;
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["jwtToken"];
        if (!string.IsNullOrEmpty(token))
        {
            await _tokenBlacklist.Add(token);
        }

        Response.Cookies.Delete("jwtToken");
        return Ok("Logout effettuato");
    }


    [HttpGet("IsAuthenticated")]
    public async Task<IActionResult> IsAuthenticated()
    {
        var token = Request.Cookies["jwtToken"];
        Console.WriteLine($"Token ricevuto: {token}");

        if (string.IsNullOrEmpty(token) || await _tokenBlacklist.IsRevoked(token))
        {
            Console.WriteLine("Token non valido o revocato");
            return Unauthorized();
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        if (jwtToken == null)
        {
            Console.WriteLine("Token non valido");
            return Unauthorized();
        }

        // Prendi tutti i ruoli dal token
        var roles = jwtToken.Claims.Where(claim => claim.Type == "role")
            .Select(claim => claim.Value)
            .ToList();

        return Ok(new { IsAuthenticated = true, Roles = roles });
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        var roles = _userManager.GetRolesAsync(user).Result;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        // Aggiungi i ruoli
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Authorize]
    [HttpPut("UpdateAccount")]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequestDto request)
    {
        var userId = _userManager.GetUserId(User);

        if (userId != request.UserId)
            return Forbid("Puoi cambiare le impostazioni dell'account solo sul tuo account");

        var validationResult = await _accountValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
            return BadRequest(new { Errors = validationErrors });
        }


        var result = await _authService.UpdateAccountAsync(request);
        
        if (!result) return BadRequest("Errore: Impossibile aggiornare il profilo");

        return Ok("Profilo aggiornato con successo!");
    }

    [Authorize]
    [HttpGet("validate-user-password")]
    public async Task<IActionResult> GetUserPassword([FromQuery] string password)
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null ) return NotFound("utente non trovato");

        bool result = await _userManager.CheckPasswordAsync(user, password);

        if(!result) return BadRequest("La password non è valida");
        return Ok("Password validata correttamente!");
    }

    [Authorize]
    [HttpPost("UpdatePassword")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestDto request)
    {
        var userId = _userManager.GetUserId(User);

        if (userId != request.UserId)
            return Forbid("Puoi cambiare la password solo sul tuo account");

        var validationResult = await _passwordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
            return BadRequest(new { Errors = validationErrors });
        }

        var result = await _authService.UpdatePasswordAsync(request);
        if (!result) return BadRequest("Errore: Impossibile aggiornare la password");

        return Ok("Password cambiata con successo");
    }

    [Authorize]
    [HttpGet("GetUser")]
    public async Task<IActionResult> GetUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return NotFound("Nessun utente trovato");

        return Ok(await _authService.GetAppUserAsync(user));
    }

    [Authorize]
    [HttpDelete("delete-user")]
    public async Task<IActionResult> DeleteUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return NotFound("Nessun utente trovato");

        await _authService.DeleteUserAsync(user);
        return Ok("Utente cancellato con successo.");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var ip = GetClientIp();

        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
            return BadRequest(new { Errors = errorMessages });
        }

        var user = await _authService.FindUserAsync(request.Email);
        if (user == null || !await _authService.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized("Credenziali non valide.");
        }

        // Controllo se l'utente ha la 2FA attivata
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return Unauthorized(new
            {
                Message = "2FA richiesto",
                Requires2FA = true,
                UserIdentifier = user.Id
            });
        }

        // Aggiorna l'IP dell'utente
        user.Ip = ip;
        await _userManager.UpdateAsync(user);

        var token = GenerateJwtToken(user);

        var clienteExists = await _dbContext.Clienti
                                            .AsNoTracking()
                                            .AnyAsync(c => c.UserId == user.Id);
        if (!clienteExists)
        {
            var cliente = new Cliente
            {
                Nome = user.Nome,
                Cognome = user.Cognome,
                Email = user.Email ?? "",
                UserId = user.Id
            };

            _dbContext.Clienti.Add(cliente);
            await _dbContext.SaveChangesAsync();
        }

        Response.Cookies.Append("jwtToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Metti true in produzione con HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return Ok(new { Message = "Utente loggato", Token = token });
    }


    [Authorize]
    [HttpPost("setup-2fa")]
    public async Task<IActionResult> Setup2FA()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return Unauthorized();

        // Genera chiave segreta 
        var key = await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        string appName = "EasyShop";
        string email = user.Email ?? "";

        string qrCode = $"otpauth://totp/{appName}:{email}?secret={key}&issuer={appName}";

        return Ok(new { Key = key, QrCode = qrCode });

    }


    [Authorize]
    [HttpPost("verify-2fa")]
    public async Task<IActionResult> Verify2A([FromBody] Verify2FARequest request)
    {
        if (!ModelState.IsValid)
            return Unauthorized();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("nessun utente trovato!");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, request.code);

        if (!isValid) return BadRequest("Il codice non è valido");

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return Ok("Autenticazione a due fattori attivata con successo!");

    }

    [HttpPost("confirm-2fa")]
    public async Task<IActionResult> Confirm2A([FromBody] Verify2FARequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Unauthorized("utente non trovato");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, request.code);
        if (!isValid) return BadRequest("codice non valido");

        var token = GenerateJwtToken(user);

        var clienteExists = await _dbContext.Clienti
                                            .AsNoTracking()
                                            .AnyAsync(c => c.UserId == user.Id);
        if (!clienteExists)
        {
            var cliente = new Cliente
            {
                Nome = user.Nome,
                Cognome = user.Cognome,
                Email = user.Email ?? "",
                UserId = user.Id
            };

            _dbContext.Clienti.Add(cliente);
            await _dbContext.SaveChangesAsync();
        }

        Response.Cookies.Append("jwtToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Metti true in produzione con HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return Ok(new { Token = token });
    }

    [HttpPost("disable-2fa")]
    [Authorize]
    public async Task<IActionResult> Disable2FA()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return NotFound("utente non trovato");

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);

        if (!result.Succeeded) return BadRequest("Qualcosa è andato storto!");

        return Ok("2FA disattivato!");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDtoRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);

        if (!result) return BadRequest("Errore generico del server.");

        return Ok("Email per il recupero password inviata.");
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        if (!await _authService.ResetPasswordAsync(request))
            return BadRequest("Errore generico del server.");

        return Ok("Password resettata correttamente");
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if(user == null)
            return NotFound("Utente non trovato");
        
        var result = await _userManager.ConfirmEmailAsync(user, token);
        
        if(result.Succeeded)
            return Ok("Email confermata. Account creato con successo!");
         
        return BadRequest("Errore nella conferma dell'email");
    }

    // ngrock
}

