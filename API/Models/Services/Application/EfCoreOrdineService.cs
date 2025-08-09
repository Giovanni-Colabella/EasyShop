using API.Models.DTO;
using API.Models.DTO.Mappings;
using API.Models.Entities;
using API.Services;

using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Application;

public class EfCoreOrdineService : IOrdineService
{

    private readonly ApplicationDbContext _context;
    public EfCoreOrdineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrdineDto>> GetOrdiniAsync()
    {
        List<Ordine> ordini = await _context.Ordini.Include(o => o.Cliente).OrderByDescending(o => o.IdOrdine).ToListAsync();
        List<OrdineDto> ordiniDto = ordini.Select(ordine => ordine.ToDto()).ToList();

        return ordiniDto;
    }


    public async Task<OrdineDto> GetOrdineAsync(int id)
    {
        Ordine ordine = await _context.Ordini
                                .Include(o => o.Cliente)
                                .FirstOrDefaultAsync(o => o.IdOrdine == id);
        if (ordine == null)
        {
            throw new KeyNotFoundException("Ordine non trovato");
        }

        return ordine.ToDto();

    }

    public async Task<bool> CreateOrdineAsync(OrdineDto ordineDto)
    {
        var altroOrdine = await _context.Ordini.FirstOrDefaultAsync(o => o.IdOrdine == ordineDto.IdOrdine);
        if (altroOrdine != null)
        {
            return false;
        }

        _context.Ordini.Add(ordineDto.ToEntity());
        await _context.SaveChangesAsync();

        return true;

    }


    public async Task<bool> DeleteOrdineAsync(int id)
    {
        var ordine = await _context.Ordini.FindAsync(id);
        if (ordine == null)
        {
            return false;
        }

        _context.Ordini.Remove(ordine);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> UpdateOrdineAsync(int id, OrdineDto ordineDto)
    {
        var ordine = await _context.Ordini.FindAsync(id);

        if (ordine == null)
        {
            return false; // Se l'ordine non esiste, restituisci false
        }

        // Aggiorna l'oggetto
        var ordineUpdated = ordineDto.ToEntity();

        // Copia le proprietà
        ordine.IdOrdine = ordineUpdated.IdOrdine;
        ordine.DataOrdine = ordineUpdated.DataOrdine;
        ordine.TotaleOrdine = ordineUpdated.TotaleOrdine;
        ordine.ClienteId = ordineUpdated.ClienteId;
        ordine.Stato = ordineUpdated.Stato;
        ordine.MetodoPagamento = ordineUpdated.MetodoPagamento;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            // Log dell'errore
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<List<OrdineDto>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<OrdineDto>(); // Se la ricerca è vuota, ritorna una lista vuota.
        }

        List<Ordine> ordiniFound = await _context.Ordini
            .Include(ordine => ordine.Cliente)  
            .Where(ordine => ordine.Cliente != null &&
                            (ordine.Cliente.Nome.Contains(keyword) ||
                            ordine.Cliente.Cognome.Contains(keyword)))
            .AsNoTracking()  
            .ToListAsync();

        List<OrdineDto> ordiniDto = ordiniFound.Select(ordine => ordine.ToDto()).ToList();

        return ordiniDto;
    }

    public async Task<int> CountAllOrdiniAsync()
    {
        return await _context.Ordini.CountAsync();
    }
}



