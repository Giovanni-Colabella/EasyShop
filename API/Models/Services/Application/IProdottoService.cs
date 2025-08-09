using API.Models.DTO;

namespace API.Models.Services.Application
{
    public interface IProdottoService
    {
        Task<List<ProdottoResponseDto>> GetProdottiAsync(int page, string search, string categoria);
        Task<ProdottoResponseDto> GetProdottoByIdAsync(int id);
        Task<int> GetTotalProdottiCountAsync(string search, string categoria);
        Task<bool> CreateProdottoAsync(ProdottoRequestDto requestDto);
    }
}
