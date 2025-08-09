using System;
using API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentExcelImportMappingDetailConfig
    : IEntityTypeConfiguration<ExcelMappingDetail>
{
    public void Configure(EntityTypeBuilder<ExcelMappingDetail> builder)
    {
        builder.HasOne(d => d.ExcelMappingHeader)
            .WithMany(h => h.ExcelMappingDetails)
            .HasForeignKey(d => d.ExcelMappingHeaderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
    }
}
