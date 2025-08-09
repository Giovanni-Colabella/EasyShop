namespace API.Models.Services.Infrastructure;

public class ImageProfilePictureService
{
    private readonly string  _updloadPath;
    public ImageProfilePictureService(IWebHostEnvironment env)
    {
        _updloadPath = Path.Combine(env.WebRootPath, "uploads/profilo");
    }

    public async Task<string> SalvaImmagineAsync(string userId, IFormFile file)
    {
        if(file == null || file.Length == 0)
            throw new ArgumentException("File non valido");
        
        var fileName = $"profilo_{userId}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_updloadPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"uploads/profilo/{fileName}"; 

    }
}
