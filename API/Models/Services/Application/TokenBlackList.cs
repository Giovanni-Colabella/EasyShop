using System.Collections.Concurrent;

namespace API.Models.Services.Application;

public class TokenBlacklist : ITokenBlacklist
{
    // Uso ConcurrentDictionary per garantire la thread-safety
    private readonly ConcurrentDictionary<string, bool> _tokenRevocati = new();

    public Task Add(string token)
    {
        _tokenRevocati.TryAdd(token, true);
        return Task.CompletedTask;
    }

    public Task<bool> IsRevoked(string token)
    {
        bool isRevoked = _tokenRevocati.ContainsKey(token);
        return Task.FromResult(isRevoked);
    }
}