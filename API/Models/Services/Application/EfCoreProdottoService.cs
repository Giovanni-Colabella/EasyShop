using API.Models.DTO;
using API.Models.DTO.Mappings;
using API.Models.Services.Infrastructure;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Application
{
    public class EfCoreProdottoService : IProdottoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImagePersister _imagePersister;
        private readonly IWebHostEnvironment _env;

        public EfCoreProdottoService(ApplicationDbContext context,
            IImagePersister imagePersister,
            IWebHostEnvironment env)
        {
            _context = context;
            _imagePersister = imagePersister;
            _env = env;
        }

        public async Task<List<ProdottoResponseDto>> GetProdottiAsync(int page, string search, string categoria)
        {
            const int pageSize = 8;
            var query = _context.Prodotti.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(prodotto => prodotto.NomeProdotto.Contains(search) ||
                                                 prodotto.Categoria.Contains(search));


            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(prodotto => prodotto.Categoria == categoria);

            var prodotti = await query.Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            var prodottiResponseDto = prodotti.Select(p => new ProdottoResponseDto
            {
                Id = p.IdProdotto,
                NomeProdotto = p.NomeProdotto,
                Prezzo = p.Prezzo,
                Categoria = p.Categoria,
                Giacenza = p.Giacenza,
                QuantitaDisponibile = p.QuantitaDisponibile,
                ImgPath = p.ImgPath
            }).ToList();

            return prodottiResponseDto;
        }

        public async Task<int> GetTotalProdottiCountAsync(string search, string categoria)
        {
            var query = _context.Prodotti.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(prodotto => prodotto.NomeProdotto.Contains(search) ||
                                                 prodotto.Categoria.Contains(search));

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(prodotto => prodotto.Categoria == categoria);

            return await query.CountAsync();
        }

        public async Task<bool> CreateProdottoAsync(ProdottoRequestDto prodottoDto)
        {

            try
            {
                var nuovoProdotto = prodottoDto.ToEntity();

                _context.Prodotti.Add(nuovoProdotto);
                await _context.SaveChangesAsync();

                if (prodottoDto.ImgFile != null)
                {
                    var imgPath = await _imagePersister.SalvaImmagineAsync(nuovoProdotto.IdProdotto, prodottoDto.ImgFile);
                    nuovoProdotto.ImgPath = imgPath;
                    await _context.SaveChangesAsync(); // Aggiorniamo il prodotto con il percorso dell'immagine
                }

                return true;
            }
            catch
            {
                return false;
            }

        }


        public async Task<ProdottoResponseDto> GetProdottoByIdAsync(int id)
        {
            var prodotto = await _context.Prodotti.FindAsync(id);

            if (prodotto != null)
                return prodotto.ToDto();

            throw new KeyNotFoundException($"Prodotto con ID {id} non trovato");
        }

    }
}
