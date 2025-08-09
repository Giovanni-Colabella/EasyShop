namespace Frontend.ViewModels;

public class ExcelImportMappingDetailModel
{
    public string ExcelColumnName { get; set; } = null!;
    public string EntityColumnName { get; set; } = null!;
}

public class ExcelMappingHeaderCreateModel
{
    public string NomeMapping { get; set; } = null!;
    public char EntityType { get; set; }    // "C" o "A"
    public List<ExcelImportMappingDetailModel> Details { get; set; } = new();
}

public class ExcelMappingHeaderUpdateModel
{
    public string NomeMapping { get; set; } = null!;
    public char EntityType { get; set; }
    public List<ExcelImportMappingDetailModel> Details { get; set; } = new();
}

public class MappingSummaryModel
{
    public int Id { get; set; }
    public string NomeMapping { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ImportErrorPreviewModel
{
    public int? RowNumber { get; set; }
    public string ExcelColumnName { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
}

public class ImportResultModel
{
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int ErrorRows { get; set; }
    public List<Dictionary<string, object>> PreviewValidRows { get; set; } = new();
    public List<ImportErrorPreviewModel> PreviewErrors { get; set; } = new();
    public int ExcelLogId { get; set; }
}

public class ExcelLogSummaryViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ExcelMappingHeaderId { get; set; }
    public char EntityType { get; set; }
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int ErrorRows { get; set; }
    public ICollection<ImportErrorPreviewModel> LogDetails { get; set; } = new List<ImportErrorPreviewModel>();

}