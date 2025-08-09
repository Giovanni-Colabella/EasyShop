using API.Models.DTO;
using API.Models.Services.Application;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanController : ControllerBase
    {
        private readonly IValidator<BanUserDto> _banUserDtoValidator;
        private readonly IUtenteBloccatoService _banUserService;
        public BanController(IValidator<BanUserDto> banUserDtoValidator,
            IUtenteBloccatoService banUserService)
        {
            _banUserDtoValidator = banUserDtoValidator;
            _banUserService = banUserService;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BanUser([FromBody] BanUserDto dto)
        {
            var validationResult = _banUserDtoValidator.Validate(dto);

            if(!validationResult.IsValid)
                return BadRequest(new { Errors = validationResult.Errors.Select( e => e.ErrorMessage )});

            return Ok(await _banUserService.BanUser(dto));
        }

    }
}
