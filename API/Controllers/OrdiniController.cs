using API.Models.DTO;
using API.Models.Services.Application;
using API.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController] 
[Authorize(Roles = "Admin")]
public class OrdiniController : ControllerBase
{

    private readonly ApplicationDbContext _context;
    private readonly IValidator<OrdineDto> _ordineDtoValidator;
    private readonly IOrdineService _ordineService;

    public OrdiniController(ApplicationDbContext context, IValidator<OrdineDto> ordineDtoValidator, IOrdineService ordineService)
    {
        _context = context;
        _ordineDtoValidator = ordineDtoValidator;
        _ordineService = ordineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrdini()
    {
        List<OrdineDto> ordini = await _ordineService.GetOrdiniAsync();

        return Ok(ordini);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrdine(int id)
    {
        try
        {
            OrdineDto ordine = await _ordineService.GetOrdineAsync(id);
            return Ok(ordine);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }   

    [HttpPost]
    public async Task<IActionResult> CreateOrdine(OrdineDto ordineDto)
    {
        var validationResult = await _ordineDtoValidator.ValidateAsync(ordineDto);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Errors = errorMessages });
        }

        bool result = await _ordineService.CreateOrdineAsync(ordineDto);
        if (!result)
        {
            return BadRequest("Ordine già presente");
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrdine(int id)
    {
        bool result = await _ordineService.DeleteOrdineAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrdine(int id, [FromBody] OrdineDto ordineDto)
    {
        var validationResult = await _ordineDtoValidator.ValidateAsync(ordineDto);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Errors = errorMessages });
        }

        bool result = await _ordineService.UpdateOrdineAsync(id, ordineDto);
        if (!result)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchOrdiniAsync([FromQuery] string keyword)
    {
        try
        {
            List<OrdineDto> ordini = await _ordineService.SearchAsync(keyword);
            return Ok(ordini);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
