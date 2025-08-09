using API.Models.DTO;
using API.Models.Services.Application;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolesService _rolesService;
        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RoleRequestDto dto)
        {
            
            return await _rolesService.AssignRoleToUser(dto) ? Ok("ruoli utenti aggiornati") 
                : BadRequest("Qualcosa è andato storto");
        }
    }
}
