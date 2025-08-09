using System.Security.Claims;
using API.Models.DTO;
using API.Models.Entities;
using API.Models.Services.Application;
using API.Models.Services.Infrastructure.Hubs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarrelloController : ControllerBase
    {

        private readonly ICarrelloService _carrelloService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<ProdottoHub> _prodottoHub;
        public CarrelloController(ICarrelloService carrelloService,
            ApplicationDbContext dbContext,
            IHubContext<ProdottoHub> prodottoHub)
        {
            _carrelloService = carrelloService;
            _dbContext = dbContext;
            _prodottoHub = prodottoHub;
        }

        [Authorize]
        [HttpGet]
        public async Task<List<ProdottoResponseDto>> GetArticoliFromCarrello()
        {
            var idCliente = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idCliente == null)
                throw new Exception("Utente non trovato");
            return await _carrelloService.GetArticoliFromCarrelloAsync(idCliente);
        }

        [Authorize]
        [HttpPost]
        public async Task AggiungiAlCarrello([FromBody] int prodottoId)
        {
            // Recupera l'id dell'utente loggato
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new Exception("Utente non trovato");
            await _carrelloService.AggiungiAlCarrelloAsync(userId, prodottoId);

        }


        [Authorize]
        [HttpDelete("{prodottoId}")]
        public async Task<bool> RimuoviDalCarrello([FromRoute] int prodottoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new Exception("Utente non trovato");
            await _carrelloService.RimuoviDalCarrelloAsync(userId, prodottoId);
            return true;
        }

        [Authorize]
        [HttpPut("aggiornaQuantitaArticolo")]
        public async Task<IActionResult> AggiornaQuantitaArticolo(int prodottoId, int quantita)
        {
            if (quantita < 1)
                return BadRequest("La quantità deve essere almeno 1");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Utente non trovato");

            var carrello = await _dbContext.Carrelli
                .Include(c => c.CarrelloProdotti)
                .FirstOrDefaultAsync(c => c.ClienteId == userId);

            if (carrello == null)
                return NotFound("Carrello non trovato per l'utente");

            var articolo = carrello.CarrelloProdotti.FirstOrDefault(a => a.ProdottoId == prodottoId);
            if (articolo == null)
                return NotFound("Articolo non trovato nel carrello");

            var prodotto = await _dbContext.Prodotti.FirstOrDefaultAsync(p => p.IdProdotto == prodottoId);
            if (prodotto == null)
                return NotFound("Prodotto non trovato");

            // Calcolo della differenza tra nuova e vecchia quantità
            int differenza = quantita - articolo.Quantita;

            // Verifica disponibilità se la nuova quantità è maggiore (sto chiedendo di più)
            if (differenza > 0 && prodotto.QuantitaDisponibile < differenza)
            {
                return BadRequest("Quantità richiesta superiore alla disponibilità del prodotto");
            }

            // Aggiorno quantità disponibile del prodotto
            prodotto.QuantitaDisponibile -= differenza;

            // Aggiorno quantità nel carrello
            articolo.Quantita = quantita;

            await _dbContext.SaveChangesAsync();

            // Notifica via SignalR
            await _prodottoHub.Clients.All.SendAsync("QuantitaDisponibileAggiornata", prodottoId, prodotto.QuantitaDisponibile);

            return Ok(new { message = "Quantità aggiornata con successo" });
        }

    }

}
