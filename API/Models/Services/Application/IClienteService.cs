namespace API.Models.Services.Application
{
    public interface IClienteService
    {
        Task<List<ClienteDto>> GetClientiAsync();
        Task<ClienteDto> GetCliente(int id);
        Task<bool> CreateClienteAsync(ClienteDto clienteDto);
        Task<ClienteDto> UpdateClienteAsync(int id, ClienteDto clienteDto);
        Task<bool> DeleteClienteAsync(int id);
        bool ClienteExists(int id);
        Task<List<ClienteDto>> SearchAsync (string keyword);
        Task<int> CountAllClientiAsync();
        Task<int> CountWeeklyClientiAsync();
        Task<List<ClienteDto>> GetClientiByEmailAsync( string search );
    }
}