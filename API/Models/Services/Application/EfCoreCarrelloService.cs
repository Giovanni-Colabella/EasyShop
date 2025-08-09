using API.Models.DTO;
using API.Models.Entities;
using API.Models.Services.Infrastructure.Hubs;
using API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Application;

public class EfCoreCarrelloService : ICarrelloService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ProdottoHub> _hubContext;
    public EfCoreCarrelloService(ApplicationDbContext context,
        IHubContext<ProdottoHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task AggiungiAlCarrelloAsync(string idCliente, int prodottoId)
    {
        var prodotto = await _context.Prodotti.FindAsync(prodottoId);
        if (prodotto == null)
            throw new Exception("Prodotto non trovato");

        if (prodotto.QuantitaDisponibile <= 0)
            throw new Exception("Prodotto non disponibile");

        var carrello = await _context.Carrelli
            .Include(c => c.CarrelloProdotti)
            .FirstOrDefaultAsync(c => c.ClienteId == idCliente);

        if (carrello == null)
        {
            carrello = new Carrello
            {
                ClienteId = idCliente,
                CarrelloProdotti = new List<CarrelloProdotto>()
            };

            _context.Carrelli.Add(carrello);
            await _context.SaveChangesAsync(); // Salviamo il carrello per ottenere l'ID
        }

        // Verifica se il prodotto è già nel carrello
        var carrelloProdotto = carrello.CarrelloProdotti
            .FirstOrDefault(cp => cp.ProdottoId == prodottoId);


        if (carrelloProdotto == null)
        {
            // Se il prodotto non è nel carrello, lo aggiungiamo
            carrelloProdotto = new CarrelloProdotto
            {
                CarrelloId = carrello.Id,
                ProdottoId = prodottoId,
            };

            _context.CarrelloProdotti.Add(carrelloProdotto);
            prodotto.QuantitaDisponibile--;
        }
        else
        {
            _context.CarrelloProdotti.Update(carrelloProdotto);
        }

        // Notifica la quantità disponibile aggiornata ai client connessi
        await _hubContext.Clients.All.SendAsync("QuantitaDisponibileAggiornata", prodottoId, prodotto.QuantitaDisponibile);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProdottoResponseDto>> GetArticoliFromCarrelloAsync(string idCliente)
    {
        var carrello = await _context.Carrelli.Include(c => c.CarrelloProdotti)
                                              .ThenInclude(cp => cp.Prodotto)
                                              .FirstOrDefaultAsync(c => c.ClienteId == idCliente);

        if (carrello == null || carrello.CarrelloProdotti.Count == 0)
            return new List<ProdottoResponseDto>();

        return carrello.CarrelloProdotti.Select(cp => new ProdottoResponseDto
        {
            Id = cp.Prodotto.IdProdotto,
            NomeProdotto = cp.Prodotto.NomeProdotto,
            Categoria = cp.Prodotto.Categoria,
            Descrizione = cp.Prodotto.Descrizione,
            ImgPath = cp.Prodotto.ImgPath,
            QuantitaDisponibile = cp.Prodotto.QuantitaDisponibile,
            Quantita = cp.Quantita,
            Giacenza = cp.Prodotto.Giacenza,
            Prezzo = cp.Prodotto.Prezzo
        }).ToList();
    }

    public async Task<bool> RimuoviDalCarrelloAsync(string idCliente, int prodottoId)
    {
        var carrello = await _context.Carrelli
            .Include(c => c.CarrelloProdotti)
            .FirstOrDefaultAsync(c => c.ClienteId == idCliente);

        if (carrello == null)
            return false;

        var carrelloProdotto = carrello.CarrelloProdotti
            .FirstOrDefault(cp => cp.ProdottoId == prodottoId);

        if (carrelloProdotto == null)
            return false;

        // Recupera il prodotto e ripristina la quantità disponibile
        var prodotto = await _context.Prodotti.FindAsync(prodottoId);
        if (prodotto != null)
        {
            prodotto.QuantitaDisponibile += carrelloProdotto.Quantita; // Ripristina tutta la quantità prenotata
        }

        _context.CarrelloProdotti.Remove(carrelloProdotto);
        await _context.SaveChangesAsync();

        // Notifica via SignalR
        if (prodotto != null)
        {
            await _hubContext.Clients.All.SendAsync("QuantitaDisponibileAggiornata", prodottoId, prodotto.QuantitaDisponibile);
        }

        return true;
    }


}