using API.Models.DTO;

namespace API.Models.Services.Application;

public interface IUtenteBloccatoService
{
    Task<bool> IsUserBanned( string nomeCliente );
    Task<bool> BanUser( BanUserDto banUserDto );
}
