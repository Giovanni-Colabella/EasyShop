using API.Models.DTO;
using API.Models.Services.Application;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilePictureController : ControllerBase
    {

        private readonly IProfilePictureService _profilePictureService;
        public ProfilePictureController(IProfilePictureService profilePictureService)
        {
            _profilePictureService = profilePictureService;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureRequest request)
        {
            try {
                return Ok(await _profilePictureService.UpdateProfilePictureAsync(request));
            } catch {
                return BadRequest("Errore generico del server");
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfilePicture([FromRoute] string userId)
        {
            try 
            {
                return Ok(await _profilePictureService.GetProfilePictureAsync(userId));
            } catch {
                return BadRequest("Errore generico del server");
            }
        }
    }
}
