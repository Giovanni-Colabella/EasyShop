namespace API.Models.DTO;

public class ExcelImportMappingDetailDto
{
    public string ExcelColumnName { get; set; } = null!;
    public string EntityColumnName { get; set; } = null!;
}

public class ExcelMappingHeaderCreateDto
{
    public string NomeMapping { get; set; } = null!;
    public char EntityType { get; set; }    // "C" o "A"
    public List<ExcelImportMappingDetailDto> Details { get; set; } = new();
}

public class ExcelMappingHeaderUpdateDto
{
    public string NomeMapping { get; set; } = null!;
    public char EntityType { get; set; } 
    public List<ExcelImportMappingDetailDto> Details { get; set; } = new();
}
public class ExcelMappingHeaderDto
{
    public int Id { get; set; }
    public string NomeMapping { get; set; } = null!;
    public string EntityType { get; set; } = null!;    // "C" o "A"
    public DateTime CreatedAt { get; set; }
    public List<ExcelImportMappingDetailDto> Details { get; set; } = new();
}
public class MappingSummaryDto
{
    public int Id { get; set; }
    public string NomeMapping { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ImportErrorPreviewDto
{
    public int RowNumber { get; set; }
    public string ExcelColumnName { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
    public string? RawValue { get; set; }
}

public class ImportExcelResponseDto
{
    public int ExcelLogId { get; set; }
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int ErrorRows { get; set; }
    public List<Dictionary<string, object>> PreviewValidRows { get; set; } = new();
}

public class ExcelErrorDetailDto
{
    public int Id { get; set; }
    public int? RowNumber { get; set; }
    public string ExcelColumnName { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
}
