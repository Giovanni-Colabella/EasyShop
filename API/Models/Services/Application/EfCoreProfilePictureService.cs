using API.Models.DTO;
using API.Models.Entities;
using API.Models.Services.Infrastructure;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Application;

public class EfCoreProfilePictureService : IProfilePictureService
{
    private readonly ImageProfilePictureService _imageService;
    private readonly ApplicationDbContext _context;
    public EfCoreProfilePictureService(ImageProfilePictureService imageService,
        ApplicationDbContext context)
    {
        _imageService = imageService;
        _context = context;
    }

    public async Task<UpdateProfilePictureResponse> GetProfilePictureAsync(string userId)
    {
        var profileImg = await _context.ProfilePictures
                                    .FirstOrDefaultAsync( pp => pp.UserId == userId);
        
        if(profileImg != null) 
        {
            return new UpdateProfilePictureResponse
            {
                UserId = userId,
                ImgPath = profileImg.ImgPath
            };
        }

        return new UpdateProfilePictureResponse();
    }

    public async Task<UpdateProfilePictureResponse> UpdateProfilePictureAsync(UpdateProfilePictureRequest request)
    {
        var oldImg = await _context.ProfilePictures
                                    .FirstOrDefaultAsync(pp => pp.UserId == request.UserId);

        if (oldImg != null)
        {
            _context.ProfilePictures.Remove(oldImg);
            await _context.SaveChangesAsync();
        }

        var imgPath = await _imageService.SalvaImmagineAsync(request.UserId, request.ImgFile);

        var nuovaImg = new ProfilePicture
        {
            UserId = request.UserId,
            ImgPath = imgPath
        };

        _context.ProfilePictures.Add(nuovaImg);
        await _context.SaveChangesAsync();

        return new UpdateProfilePictureResponse
        {
            UserId = request.UserId,
            ImgPath = imgPath
        };
    }
}
