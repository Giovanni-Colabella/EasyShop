using System;
using API.Models.DTO;

namespace API.Models.Services.Application;

public interface IProfilePictureService
{
    Task<UpdateProfilePictureResponse> GetProfilePictureAsync(string userId);
    Task<UpdateProfilePictureResponse> UpdateProfilePictureAsync(UpdateProfilePictureRequest request);
}
