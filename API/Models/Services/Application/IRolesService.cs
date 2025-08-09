using API.Models.DTO;

namespace API.Models.Services.Application;

public interface IRolesService
{
    Task<bool> AssignRoleToUser(RoleRequestDto dto);
}
