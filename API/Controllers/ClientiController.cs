using API.Models;
using API.Models.Services.Application;
using API.Models.ValueObjects;
using API.Services;

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ClientiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<ClienteDto> _clienteDtoValidator;
        private readonly IClienteService _clienteService;

        public ClientiController(
            ApplicationDbContext context,
            IValidator<ClienteDto> clienteDtoValidator,
            IClienteService clienteService)
        {
            _context = context;
            _clienteDtoValidator = clienteDtoValidator;
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetClienti()
        {
            List<ClienteDto> clienti = await _clienteService.GetClientiAsync();
            return Ok(clienti);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCliente(int id)
        {
            ClienteDto cliente = await _clienteService.GetCliente(id);

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCliente(ClienteDto clienteDto)
        {
            var validationResult = await _clienteDtoValidator.ValidateAsync(clienteDto);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errorMessages });
            }

            var result = await _clienteService.CreateClienteAsync(clienteDto);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditCliente(int id, [FromBody] ClienteDto clienteDto)
        {

            var validationResult = await _clienteDtoValidator.ValidateAsync(clienteDto);

            if (!validationResult.IsValid)
            {
                // Ritorna un BadRequest con solo i messaggi di errore.
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

                return BadRequest(new { Errors = errorMessages });
            }


            var updatedCliente = await _clienteService.UpdateClienteAsync(id, clienteDto);
            return Ok(updatedCliente);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var result = await _clienteService.DeleteClienteAsync(id);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            try
            {
                var clienti = await _clienteService.SearchAsync(keyword);
                return Ok(clienti);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("genera")]
        public IActionResult GeneraClientiRandom()
        {

            var clientiGenerati = GeneratoreDati.GeneraClienti(357);

            var clienti = clientiGenerati.Select(cg => new Cliente
            {
                Nome = cg.Nome,
                Cognome = cg.Cognome,
                Email = cg.Email,
                NumeroTelefono = cg.NumeroTelefono,
                Indirizzo = cg.Indirizzo,
                DataIscrizione = cg.DataIscrizione
            }).ToList();


            _context.AddRange(clienti);

            _context.SaveChanges();

            // Ritorna i clienti appena generati
            return Ok(clienti);
        }


        [HttpGet("email")]
        public async Task<IActionResult> GetClientiByEmailAsync([FromQuery] string search)
        {
            return Ok(await _clienteService.GetClientiByEmailAsync(search));
        }


    }
}
