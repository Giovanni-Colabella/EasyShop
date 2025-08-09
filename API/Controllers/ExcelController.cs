using API.Models.DTO;
using API.Models.Entities;
using API.Models.Services.Infrastructure;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly ExcelImportService _excelReadService;
        private readonly ILogger<ExcelController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExcelController(
            ExcelImportService excelReadService,
            ApplicationDbContext dbContext,
            ILogger<ExcelController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _excelReadService = excelReadService;
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
        }


        [HttpPost("upload-excel")]
        public async Task<IActionResult> ImportExcel(
                    [FromForm] IFormFile file,
                    [FromForm] int mappingHeaderId,
                    [FromForm] char entityType)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File non fornito o vuoto.");

            try
            {
                using var stream = file.OpenReadStream();

                var result = await _excelReadService.ImportExcelAsync(
                    stream,
                    mappingHeaderId,
                    entityType,
                    file.FileName
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Errore durante l'import excel");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Errore durante l'importazione del file Excel.",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("errors/{excelLogId}")]
        public async Task<IActionResult> GetErrorById(int excelLogId)
        {
            var log = await _dbContext.ExcelLogDetails
                .Where(e => e.ExcelLogId == excelLogId)
                .Select(e => new ExcelErrorDetailDto
                {
                    Id = e.Id,
                    ExcelColumnName = e.ExcelColumnName,
                    ErrorMessage = e.ErrorMessage,
                    RowNumber = e.RowNumber
                })
                .ToListAsync();

            return Ok(log);
        }


        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors()
        {
            var errors = await _dbContext.ExcelLogs.ToListAsync();
            return Ok(errors);
        }

        [HttpGet("get-mappings")]
        public async Task<ActionResult<List<MappingSummaryDto>>> GetMappings()
        {
            var mappings = await _dbContext.ExcelMappingHeaders
                .Select(h => new MappingSummaryDto
                {
                    Id = h.Id,
                    NomeMapping = h.NomeMapping,
                    CreatedAt = h.CreatedAt,
                    EntityType = h.EntityType.ToString()
                }).ToListAsync();

            return Ok(mappings);
        }

        [HttpGet("get-mappings/{id}")]
        public async Task<ActionResult<ExcelMappingHeaderCreateDto>> GetMappingById(int id)
        {
            var mapping = await _dbContext.ExcelMappingHeaders
                .Include(h => h.ExcelMappingDetails)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (mapping == null)
                return NotFound();

            var dto = new ExcelMappingHeaderCreateDto
            {
                NomeMapping = mapping.NomeMapping,
                EntityType = mapping.EntityType,
                Details = mapping.ExcelMappingDetails.Select(d => new ExcelImportMappingDetailDto
                {
                    ExcelColumnName = d.ExcelColumnName,
                    EntityColumnName = d.EntityColumnName
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost("create-mapping")]
        public async Task<ActionResult> CreateMapping([FromBody] ExcelMappingHeaderCreateDto createDto)
        {
            var mapping = new ExcelMappingHeader
            {
                NomeMapping = createDto.NomeMapping,
                EntityType = createDto.EntityType,
                UserId = _userManager.GetUserId(User) ?? throw new Exception(),
                ExcelMappingDetails = createDto.Details.Select(d => new ExcelMappingDetail
                {
                    ExcelColumnName = d.ExcelColumnName,
                    EntityColumnName = d.EntityColumnName,
                }).ToList()
            };

            _dbContext.ExcelMappingHeaders.Add(mapping);
            await _dbContext.SaveChangesAsync();

            return Ok("mapping creato con successo");
        }

        [HttpPut("update-mappings/{id}")]
        public async Task<IActionResult> UpdateMapping(
            int id,
            [FromBody] ExcelMappingHeaderUpdateDto updateDto)
        {
            // 1) Trova l’header esistente
            var existing = await _dbContext.ExcelMappingHeaders
                .Include(h => h.ExcelMappingDetails)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (existing == null)
                return NotFound();

            existing.NomeMapping = updateDto.NomeMapping;
            existing.EntityType = updateDto.EntityType;

            _dbContext.ExcelMappingDetails.RemoveRange(existing.ExcelMappingDetails);

            var nuoviDettagli = updateDto.Details.Select(d => new ExcelMappingDetail
            {

                ExcelMappingHeaderId = existing.Id,

                ExcelColumnName = d.ExcelColumnName,
                EntityColumnName = d.EntityColumnName
            }).ToList();


            await _dbContext.ExcelMappingDetails.AddRangeAsync(nuoviDettagli);

            existing.ExcelMappingDetails = nuoviDettagli;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

    }

}
