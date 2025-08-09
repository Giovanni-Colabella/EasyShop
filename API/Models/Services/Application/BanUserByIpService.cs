using API.Models.DTO;
using API.Models.Entities;
using API.Services;

using Microsoft.AspNetCore.Identity;

namespace API.Models.Services.Application;

public class BanUserByIpService : IUtenteBloccatoService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context; 
    public BanUserByIpService(UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public async Task<bool> BanUser(BanUserDto banUserDto)
    {
        var user = await _userManager.FindByEmailAsync(banUserDto.Email);

        if( user == null ) return false;

        var nuovoBan = new BannedIp
        {
            Ip = user.Ip,
            Motivazione = banUserDto.Motivazione,
        };

        user.ChangeStatus(Enums.ApplicationUserStatus.Bannato); // aggiorna lo stato utente a Bannato

        _context.BannedIps.Add(nuovoBan);
        await _context.SaveChangesAsync();

        return true;
    }

    public Task<bool> IsUserBanned(string nomeCliente)
    {
        throw new NotImplementedException();
    }
}
