using API.Models.DTO;
using API.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Models.Services.Application;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterRequestDto request);
    Task<ApplicationUser> FindUserAsync(string email);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<ApplicationUserDto> GetAppUserAsync(ApplicationUser user);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<bool> UpdateAccountAsync(UpdateAccountRequestDto dto);
    Task<bool> UpdatePasswordAsync(UpdatePasswordRequestDto dto);
    Task DeleteUserAsync(ApplicationUser user);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDtoRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordDto request);
}
