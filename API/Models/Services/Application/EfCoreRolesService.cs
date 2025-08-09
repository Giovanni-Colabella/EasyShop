using System;
using API.Models.DTO;
using API.Models.Entities;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Services.Application;

public class EfCoreRolesService : IRolesService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EfCoreRolesService(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context=context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> AssignRoleToUser(RoleRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user == null)
        {
            return false;
        }

        // Crea il ruolo se non esiste 
        if(!await _roleManager.RoleExistsAsync(dto.Role))
        {
            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
            if(!createRoleResult.Succeeded)
            {
                return false;
            }
        }

        var ruoliCorrenti = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, ruoliCorrenti);
        var result = await _userManager.AddToRoleAsync(user, dto.Role);

        if(!result.Succeeded)
        {
            return false;
        }

        return true;
    }
}
