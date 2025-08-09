using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheck : ControllerBase
    {
        private readonly IConfiguration _config;
        public HealthCheck(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("isMaintenanceModeEnabled")]
        public async Task<IActionResult> Maintenance()
        {
            try{
                
                var manutenzioneAttiva = bool.Parse(_config["Maintenance"]);

                if(manutenzioneAttiva)
                {
                    return StatusCode(503, new {
                        message = "Il sito è in manutenzione"
                    });
                }

                return Ok("Il sito è funzionante");
            } catch (Exception e)
            {
                return StatusCode(503,new {
                    message = "Errore nel verificare lo stato della manutenzione",
                });
            }
        }
    }
}
