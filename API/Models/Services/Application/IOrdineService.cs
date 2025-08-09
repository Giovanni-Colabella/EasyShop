using System;
using API.Models.DTO;

namespace API.Models.Services.Application;

public interface IOrdineService
{
    Task<List<OrdineDto>> GetOrdiniAsync();
    Task<OrdineDto> GetOrdineAsync(int id);
    Task<bool> CreateOrdineAsync(OrdineDto ordineDto);
    Task<bool> DeleteOrdineAsync(int id);
    Task<bool> UpdateOrdineAsync(int id, OrdineDto ordineDto);
    Task<List<OrdineDto>> SearchAsync(string keyword);
    Task<int> CountAllOrdiniAsync();
}
