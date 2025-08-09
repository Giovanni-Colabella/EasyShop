using API.Models.DTO;

namespace API.Models.Services.Application;

public interface ICarrelloService
{
    Task AggiungiAlCarrelloAsync(string idCliente, int prodottoId);
    Task<List<ProdottoResponseDto>> GetArticoliFromCarrelloAsync(string idCliente);
    Task<bool> RimuoviDalCarrelloAsync(string idCliente, int prodottoId);
}
