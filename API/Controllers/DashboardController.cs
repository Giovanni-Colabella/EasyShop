using API.Models.Services.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly IOrdineService _ordineService;
        public DashboardController(
            IClienteService clienteService,
            IOrdineService ordineService)
        {
            _clienteService = clienteService;
            _ordineService = ordineService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int allClienti = await _clienteService.CountAllClientiAsync();
            int allOrdini = await _ordineService.CountAllOrdiniAsync();
            int weeklyClienti = await _clienteService.CountWeeklyClientiAsync();

            var stats = new {
                AllClienti = allClienti,
                AllOrdini = allOrdini,
                WeeklyClienti = weeklyClienti
            };

            return Ok(stats);
        }
    }
}