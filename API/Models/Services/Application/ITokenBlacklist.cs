namespace API.Models.Services.Application;

public interface ITokenBlacklist
{
    Task<bool> IsRevoked(string token);
    Task Add(string token);
}