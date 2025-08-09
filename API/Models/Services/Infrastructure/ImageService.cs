
namespace API.Models.Services.Infrastructure
{
    public class ImageService : IImagePersister
    {
        private readonly string _updloadPath;
        public ImageService(IWebHostEnvironment env)
        {
            _updloadPath = Path.Combine(env.WebRootPath, "uploads/prodotti");
        }
        public async Task<string> SalvaImmagineAsync(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File non valido");

            var fileName = $"prod_{id}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_updloadPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"uploads/prodotti/{fileName}";
        }
    }
}
