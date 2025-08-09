using System.Net.Http.Headers;
using API.Models.DTO;
using API.Models.Entities;
using API.Models.Enums;
using API.Models.Services.Application;
using API.Models.Services.Infrastructure;
using API.Models.Services.Infrastructure.Hubs;
using API.Services;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PaymentController : ControllerBase
    {
        private readonly PayPalService _payPalService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<ProdottoHub> _prodottoHub;
        private readonly HttpClient _httpClient;
        private readonly IPdfService _pdfService;
        private readonly IEmailSender _emailService;

        public PaymentController(
            PayPalService payPalService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IHubContext<ProdottoHub> prodottoHub,
            HttpClient httpClient,
            IPdfService pdfService,
            IEmailSender emailSender)
        {
            _payPalService = payPalService;
            _userManager = userManager;
            _dbContext = dbContext;
            _prodottoHub = prodottoHub;
            _httpClient = httpClient;
            _pdfService = pdfService;
            _emailService = emailSender;
        }

        // POST: api/payment/create
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] PagamentoRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var clienteId = await _dbContext.Clienti
                .Where(c => c.UserId == user.Id)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (clienteId == 0)
                return BadRequest(new { Errors = new[] { "Cliente non trovato" } });

            var prodottoIds = request.Prodotti.Select(p => p.Id).ToList();
            var prodotti = request.Prodotti.Where(p => prodottoIds.Contains(p.Id)).ToList();
            var dettagliOrdini = await _dbContext.Prodotti
                .Where(p => prodottoIds.Contains(p.IdProdotto))
                .Select(p => new DettaglioOrdine
                {
                    ProdottoId = p.IdProdotto,
                    Quantita = 1
                }).ToListAsync();
            
            if (!dettagliOrdini.Any())
                return BadRequest(new { Errors = new[] { "Prodotti non validi" } });

            // Crea ordine in DB
            var ordine = new Ordine
            {
                ClienteId = clienteId,
                DataOrdine = DateTime.UtcNow,
                TotaleOrdine = request.Amount,
                MetodoPagamento = "PayPal",
                Stato = OrdineStatus.Sospeso,
                DettagliOrdini = dettagliOrdini,
                IndirizzoSpedizione = (await _dbContext.Clienti.Select(c => new
                {
                    Indirizzo = c.Indirizzo.Citta + " " + c.Indirizzo.Via + " " + c.Indirizzo.CAP + " " + c.Indirizzo.HouseNumber
                }).FirstOrDefaultAsync())?.Indirizzo,
                PayPalOrdineId = null // Inizialmente nullo, verrà aggiornato dopo la creazione dell'ordine PayPal
            };
            _dbContext.Ordini.Add(ordine);
            await _dbContext.SaveChangesAsync();

            // Chiama il servizio PayPal
            var serviceRequest = new PagamentoRequest
            {
                Prodotti = prodotti,
                Amount = request.Amount,
                Currency = request.Currency,
                ReturnUrl = request.ReturnUrl + $"?orderId={ordine.IdOrdine}",
                CancelUrl = request.CancelUrl
            };
            var paypalOrderId = await _payPalService.CreateOrderAsync(serviceRequest);

            if (string.IsNullOrEmpty(paypalOrderId))
                return BadRequest(new { Errors = new[] { "Errore durante la creazione dell'ordine PayPal" } });

            // Aggiorna l'ordine con l'ID dell'ordine PayPal
            ordine.PayPalOrdineId = paypalOrderId;
            _dbContext.Ordini.Update(ordine);
            await _dbContext.SaveChangesAsync();

            return Ok(new { OrderId = paypalOrderId });
        }

        // POST: api/payment/capture
        [HttpPost("capture")]
        public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentRequest request)
        {
            try
            {
                // 1. Chiama PayPal per catturare il pagamento
                var result = await _payPalService.CaptureOrderAsync(request.Token);

                if (result == null)
                    return BadRequest(new { Errors = new[] { "Errore durante la cattura del pagamento" } });

                // 2. Recupera l'ordine dal database usando il token PayPal (assumendo che PayPalOrderId sia salvato)
                var ordine = await _dbContext.Ordini
                    .Include(o => o.Cliente)
                    .Include(o => o.DettagliOrdini)
                        .ThenInclude(d => d.Prodotto)
                    .FirstOrDefaultAsync(o => o.PayPalOrdineId == request.Token);

                if (ordine == null)
                    return NotFound(new { Errors = new[] { "Ordine non trovato" } });

                var pdfBytes = _pdfService.GenerateOrderReceipt(ordine);
                await _emailService.SendEmailWithAttachmentAsync(ordine.Cliente.Email,
                    "Conferma Ordine",
                    "In allegato, la ricevuta.",
                    pdfBytes,
                    $"Ricevuta_Ordine_{ordine.IdOrdine}.pdf");

                // 3. Aggiorna stato ordine
                ordine.Stato = OrdineStatus.InElaborazione;

                // 4. Scala quantità disponibile dei prodotti
                foreach (var dettaglio in ordine.DettagliOrdini)
                {
                    dettaglio.Prodotto.Giacenza -= dettaglio.Quantita;
                    if (dettaglio.Prodotto.Giacenza < 0)
                        dettaglio.Prodotto.Giacenza = 0;

                    dettaglio.Prodotto.QuantitaDisponibile -= dettaglio.Quantita;
                    if (dettaglio.Prodotto.QuantitaDisponibile < 0)
                        dettaglio.Prodotto.QuantitaDisponibile = 0;

                    await _prodottoHub.Clients.All.SendAsync("QuantitaDisponibileAggiornata", dettaglio.ProdottoId, dettaglio.Prodotto.QuantitaDisponibile);
                    await _prodottoHub.Clients.All.SendAsync("QuantitaMagazzinoAggiornata", dettaglio.ProdottoId, dettaglio.Prodotto.Giacenza);

                }

                // 5. Salva modifiche
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Pagamento catturato e quantità aggiornata",
                    OrdineId = ordine.IdOrdine,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la cattura del pagamento: {ex.Message}");
                return StatusCode(500, new { Errors = new[] { "Errore interno del server: " + ex.Message } });
            }

        }

        // [HttpGet("get-payment-id")]
        // [Authorize]
        // public async Task<IActionResult> GetPaymentById(int ordineId)
        // {
        //     var user = await _userManager.GetUserAsync(User);
        //     if (user == null)
        //         return Unauthorized();

        //     var orderId = await _dbContext.Ordini
        //         .Where(o => o.IdOrdine == ordineId && o.Cliente.UserId == user.Id)
        //         .Select(o => o.IdOrdine)
        //         .FirstOrDefaultAsync();

        //     if (ordineId == 0)
        //         return NotFound(new { Errors = new[] { "Ordine non trovato" } });
        //     return Ok(new { OrderId = orderId });
        // }

        [HttpPost("payment-from-cart")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentFromCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var clienteId = await _dbContext.Clienti
                .Where(c => c.UserId == user.Id)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (clienteId == 0)
                return BadRequest(new { Errors = new[] { "Cliente non trovato" } });

            var carrello = await _dbContext.Carrelli
                .Include(c => c.CarrelloProdotti)
                    .ThenInclude(cp => cp.Prodotto)
                .FirstOrDefaultAsync(c => c.ClienteId == user.Id);

            if (carrello == null || carrello.CarrelloProdotti.Count == 0)
                return BadRequest(new { Errors = new[] { "Carrello vuoto" } });

            // Verifica disponibilità
            foreach (var item in carrello.CarrelloProdotti)
            {
                if (item.Quantita > item.Prodotto.QuantitaDisponibile)
                {
                    return BadRequest(new { Errors = new[] { $"Prodotto {item.Prodotto.NomeProdotto} non disponibile nella quantità richiesta." } });
                }
            }

            decimal totaleOrdine = carrello.CarrelloProdotti
                .Sum(item => item.Prodotto.Prezzo * item.Quantita);

            var dettagliOrdini = carrello.CarrelloProdotti.Select(item => new DettaglioOrdine
            {
                ProdottoId = item.Prodotto.IdProdotto,
                Quantita = item.Quantita
            }).ToList();

            var ordine = new Ordine
            {
                ClienteId = clienteId,
                DataOrdine = DateTime.UtcNow,
                TotaleOrdine = totaleOrdine,
                MetodoPagamento = "PayPal",
                Stato = OrdineStatus.Sospeso,
                DettagliOrdini = dettagliOrdini,
                IndirizzoSpedizione = await _dbContext.Clienti
                    .Where(c => c.Id == clienteId)
                    .Select(c => c.Indirizzo.Citta + " " + c.Indirizzo.Via + " " + c.Indirizzo.CAP + " " + c.Indirizzo.HouseNumber)
                    .FirstOrDefaultAsync()
            };

            _dbContext.Ordini.Add(ordine);
            await _dbContext.SaveChangesAsync();

            var prodottiForPayment = carrello.CarrelloProdotti
                .Select(item => new ProdottoPaymentRequestDto
                {
                    Id = item.Prodotto.IdProdotto,
                    Quantita = item.Quantita
                }).ToList();

            PagamentoRequest pagamentoRequest = new PagamentoRequest
            {
                Prodotti = prodottiForPayment,
                Amount = totaleOrdine,
                Currency = "EUR",
                ReturnUrl = "http://localhost:5100/Payment/PayPal/PayPalConfirmed",
                CancelUrl = "http://localhost:5100/Payment/PayPal/Cancel"
            };

            var paypalOrderId = await _payPalService.CreateOrderAsync(pagamentoRequest);

            ordine.PayPalOrdineId = paypalOrderId;
            _dbContext.Ordini.Update(ordine);

            await _dbContext.SaveChangesAsync();

            return Ok(new { OrderId = paypalOrderId });

        }

        [HttpGet("get-my-orders")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var associatedClienteId = await _dbContext.Clienti
                .Where(c => c.UserId == user.Id)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (associatedClienteId == 0)
                return BadRequest(new { Errors = new[] { "Cliente non trovato" } });

            var ordini = await _dbContext.Ordini
                .Where(ordine => ordine.ClienteId == associatedClienteId)
                .Include(ordine => ordine.DettagliOrdini)
                    .ThenInclude(d => d.Prodotto)
                .ToListAsync();

            if (ordini == null || !ordini.Any())
                return NotFound(new { Errors = new[] { "Nessun ordine trovato" } });

            var response = ordini.Select(
                ordine => new
                {
                    ordine.IdOrdine,
                    ordine.DataOrdine,
                    ordine.TotaleOrdine,
                    ordine.MetodoPagamento,
                    ordine.Stato,
                    DettaglioOrdine = ordine.DettagliOrdini.Select(
                        d => new
                        {
                            d.ProdottoId,
                            d.Prodotto.NomeProdotto,
                            d.Prodotto.Prezzo,
                            d.Quantita
                        }
                    ).ToList()
                }
            ).ToList();

            return Ok(response);
        }
    }
}

