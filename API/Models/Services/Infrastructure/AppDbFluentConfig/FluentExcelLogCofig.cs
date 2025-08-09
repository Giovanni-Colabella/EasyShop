using System;
using API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentExcelLogCofig : IEntityTypeConfiguration<ExcelLog>
{
    public void Configure(EntityTypeBuilder<ExcelLog> builder)
    {
        builder.HasOne(l => l.ExcelMappingHeader)
            .WithMany(h => h.Logs)
            .HasForeignKey(l => l.ExcelMappingHeaderId);
    }
}
