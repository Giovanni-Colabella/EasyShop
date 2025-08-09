namespace API.Models.Services.Infrastructure
{
    public interface IImagePersister
    {
        Task<string> SalvaImmagineAsync(int id, IFormFile file);
    }
}
