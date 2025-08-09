using API.Models.Entities;
using API.Models.Services.Application;
using API.Models.Services.Infrastructure;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public PdfController(
            IPdfService pdfService,
            IEmailSender emailSender,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _pdfService = pdfService;
            _emailSender = emailSender;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet("get-order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            var order = await _dbContext.Ordini
                .Include(o => o.Cliente)
                .Include(o => o.DettagliOrdini)
                    .ThenInclude(d => d.Prodotto)
                .FirstOrDefaultAsync(o => o.IdOrdine == orderId);
                

            if (order == null)
            {
                return NotFound("Nessun ordine trovato");
            }

            var pdfBytes = _pdfService.GenerateOrderReceipt(order);

            // Invia mail
            await _emailSender.SendEmailWithAttachmentAsync(order.Cliente.Email,
                "Conferma Ordine",
                "In allegato, la ricevuta.",
                pdfBytes,
                $"Ricevuta_Ordine_{order.IdOrdine}.pdf");

            // Preview del PDF 
            return File(pdfBytes, "application/pdf", $"Ricevuta_Ordine_{order.IdOrdine}.pdf");
        }

        [HttpGet("show-order/{orderId}")]
        public async Task<IActionResult> ShowOrder(int orderId)
        {
            var order = await _dbContext.Ordini
                .Include(o => o.Cliente)
                .Include(o => o.DettagliOrdini)
                    .ThenInclude(d => d.Prodotto)
                .FirstOrDefaultAsync(o => o.IdOrdine == orderId);

            if (order == null) 
            {
                return NotFound("Nessun ordine trovato");
            }

            var pdfBytes = _pdfService.GenerateOrderReceipt(order);
            return File(pdfBytes, "application/pdf");
        }


        [HttpGet("get-clienti")]
        [Authorize]
        public async Task<IActionResult> GetClienti([FromQuery] int count = 100, char order = 'd')
        {
            if(count <= 0 || count > 10000)
                return BadRequest("Il parametro 'count' deve essere compreso tra 1 e 10000");
            if (order != 'a' && order != 'd')
                return BadRequest("Il parametro 'order' deve essere 'a' (ascendente) o 'd' (discendente)");

            var query = order == 'a'
                ? _dbContext.Clienti.OrderBy(c => c.Id)
                : _dbContext.Clienti.OrderByDescending(c => c.Id);

            var clienti = await query.Take(count).ToListAsync();

            var pdfBytes = _pdfService.GenerateClientList(clienti);

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound("Non è stato possibile autenticare la richiesta");

            return File(pdfBytes, "application/pdf", $"Elenco_clienti");
        }

        [HttpGet("get-prodotti")]
        [Authorize]
        public async Task<IActionResult> GetProdotti()
        {
            var prodotti = await _dbContext.Prodotti.ToListAsync();
            var pdfBytes = _pdfService.GenerateProductList(prodotti);
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound("Non è stato possibile autenticare la richiesta");

            return File(pdfBytes, "application/pdf", $"Elenco_prodotti");
        }

        [HttpGet("get-report-vendite")]
        [Authorize]
        public async Task<IActionResult> GetReportVendite(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user is null) return Unauthorized("Non è stato possibile autenticare la richiesta");

            // Imposta i limiti di data se non specificati
            var start = fromDate ?? DateTime.MinValue;
            var end = toDate ?? DateTime.MaxValue;

            var vendite = await _dbContext.Ordini
                .Include(o => o.Cliente)
                .Include(o => o.DettagliOrdini)
                    .ThenInclude(d => d.Prodotto)
                .Where(o => o.DataOrdine >= start && o.DataOrdine <= end)
                .OrderByDescending(o => o.DataOrdine)
                .ToListAsync();

            if(!vendite.Any())
                return NotFound("Nessuna vendita trovata");

            // Genera il documento PDF con QuestPdf
            var pdfBytes = _pdfService.GenerateSalesReport(vendite, start, end);

            return File(pdfBytes, "application/pdf", $"Report_Vendite_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}
