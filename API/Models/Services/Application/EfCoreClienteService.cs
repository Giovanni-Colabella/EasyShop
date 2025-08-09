using API.Models.DTO.Mappings;
using API.Models.Entities;
using API.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Application
{
    public class EfCoreClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public EfCoreClienteService(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> CreateClienteAsync(ClienteDto clienteDto)
        {
            var altroCliente = await _context.Clienti.FirstOrDefaultAsync(c => c.Email == clienteDto.Email);
            if (altroCliente != null)
            {
                return false;
            }

            Cliente cliente = clienteDto.ToEntity();

            _context.Clienti.Add(cliente);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClienteAsync(int id)
        {
            var cliente = await _context.Clienti.FindAsync(id);
            if (cliente == null)
            {
                return false;
            }

            _context.Clienti.Remove(cliente);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ClienteDto> GetCliente(int id)
        {
            var cliente = await _context.Clienti.FindAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente non trovato");
            }

            return cliente.ToDto();
        }

        public async Task<List<ClienteDto>> GetClientiAsync()
        {
            // Aspettiamo la query asincrona
            var clienti = await _context.Clienti.OrderByDescending(c => c.Id).ToListAsync();

            // Per ogni cliente andiamo a creare il corrispettivo DTO
            List<ClienteDto> clientiDto = clienti.Select(cliente =>
                cliente.ToDto()
            ).ToList();

            // Restituisci la lista di DTO
            return clientiDto;
        }


        public async Task<ClienteDto> UpdateClienteAsync(int id, ClienteDto clienteDto)
        {
            // Trova il cliente dal database usando l'ID
            var cliente = await _context.Clienti.FindAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente non trovato");
            }

            var updatedCliente = clienteDto.ToEntity();


            cliente.Nome = updatedCliente.Nome;
            cliente.Cognome = updatedCliente.Cognome;
            cliente.Email = updatedCliente.Email;
            cliente.NumeroTelefono = updatedCliente.NumeroTelefono;
            cliente.Indirizzo = updatedCliente.Indirizzo;
            cliente.DataIscrizione = updatedCliente.DataIscrizione;


            // Salva le modifiche nel database
            await _context.SaveChangesAsync();

            // Restituisce il DTO aggiornato
            return cliente.ToDto();
        }

        public bool ClienteExists(int id)
        {
            // Controlla se esiste un cliente con l'id specificato
            return _context.Clienti.Any(c => c.Id == id);
        }

        public async Task<List<ClienteDto>> SearchAsync(string keyword)
        {
            List<Cliente> clientiFound = await _context.Clienti.Where(c => c.Nome.Contains(keyword)
                                                                    || c.Cognome.Contains(keyword)
                                                                    || c.Email.Contains(keyword)
                                                                    ).ToListAsync();

            List<ClienteDto> clientiDto = clientiFound.Select(c => c.ToDto()).ToList();

            return clientiDto;
        }

        public async Task<int> CountAllClientiAsync()
        {
            return await _context.Clienti.CountAsync();
        }

        public async Task<int> CountWeeklyClientiAsync()
        {
            return await _context.Clienti.Where(c => c.DataIscrizione >= DateTime.Now.AddDays(-7)).CountAsync();
        }

        public async Task<List<ClienteDto>> GetClientiByEmailAsync(string search)
        {
            var clientiTrovati =  await _context.Clienti.Where( cliente => cliente.Email.Contains(search)).Take(3).ToListAsync();

            List<ClienteDto> clientiDto = clientiTrovati.Select( cliente => cliente.ToDto()).ToList();
            return clientiDto;
        }
   
    }
}