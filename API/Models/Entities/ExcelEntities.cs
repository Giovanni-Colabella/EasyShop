using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities;

public class ExcelMappingHeader
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string NomeMapping { get; set; } = null!;
    [Required]
    [Column(TypeName = "char(1)")]
    public char EntityType { get; set; } 
    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExcelMappingDetail> ExcelMappingDetails { get; set; } = new List<ExcelMappingDetail>();
    public ICollection<ExcelLog> Logs { get; set; } = new List<ExcelLog>();
}

public class ExcelMappingDetail
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(ExcelMappingHeader))]
    [Required]
    public int ExcelMappingHeaderId { get; set; }
    public ExcelMappingHeader ExcelMappingHeader { get; set; } = null!;
    [Required]
    public string ExcelColumnName { get; set; } = null!;
    [Required]
    public string EntityColumnName { get; set; } = null!;

}

public class ExcelLog
{
    [Key]
    public int Id { get; set; }
    [Required]
    [ForeignKey(nameof(ExcelMappingHeader))]
    public int ExcelMappingHeaderId { get; set; }
    [Required]
    [Column(TypeName = "char(1)")]
    public char EntityType { get; set; }
    [Required]
    public string FileName { get; set; } = null!;
    public int TotalRows { get; set; } = 0;
    public int SuccessRows { get; set; } = 0;
    public int ErrorRows { get; set; } = 0;

    public ExcelMappingHeader ExcelMappingHeader { get; set; } = null!;
    public ICollection<ExcelLogDetail> LogDetails { get; set; } = null!;
}

public class ExcelLogDetail
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int ExcelLogId { get; set; }
    public ExcelLog ExcelLog { get; set; } = null!;
    [Required]
    public string ExcelColumnName { get; set; } = null!;
    [Required]
    public string ErrorMessage { get; set; } = null!;
    public int? RowNumber { get; set; } = null;
}