using System.Net;
using API.Models.DTO;
using API.Models.Services.Application;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ProdottiController : ControllerBase
    {
        private readonly IProdottoService _service;
        private readonly IValidator<ProdottoRequestDto> _requestValidator;

        public ProdottiController(
            IProdottoService service,
            IValidator<ProdottoRequestDto> requestValidator)
        {
            _service = service;
            _requestValidator = requestValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProdotti([FromQuery] int page = 1, [FromQuery] string search = "",
            [FromQuery] string categoria = "")
        {
            if (page < 1)
            {
                return BadRequest("La pagina deve essere maggiore di 0");
            }

            // sanitizzazione della stringa di ricerca 
            search = WebUtility.HtmlEncode(search).Trim();
            categoria = WebUtility.HtmlEncode(categoria).Trim();

            var prodotti = await _service.GetProdottiAsync(page, search, categoria);
            var totaleProdotti = await _service.GetTotalProdottiCountAsync(search, categoria);


            var json = new
            {
                TotalCount = totaleProdotti,
                PaginaCorrente = page,
                TotalePagine = (int)Math.Ceiling(totaleProdotti / (double)8),
                Items = prodotti
            };
            return Ok(json);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProdottoById(int id)
        {
            var prodotto = await _service.GetProdottoByIdAsync(id);
            if (prodotto == null)
            {
                return NotFound();
            }
            return Ok(prodotto);
        }

        [HttpPost]
        public async Task<IActionResult> CreaProdotto([FromForm] ProdottoRequestDto request)
        {
            // Validazione
            var validationResult = await _requestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                // Gestione degli errori di validazione
                var errors = validationResult.Errors
                    .Select(e => new { e.ErrorMessage })
                    .ToList();
                return BadRequest(errors);
            }

            try
            {
                // Usa il service per creare il prodotto
                var result = await _service.CreateProdottoAsync(request);
                return Ok("Prodotto creato con successo");
            }
            catch (Exception ex)
            {
                // Gestione di altri errori
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }

    }
}
