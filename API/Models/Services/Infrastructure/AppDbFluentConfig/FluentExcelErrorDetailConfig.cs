using System;
using API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentExcelErrorDetailConfig
    : IEntityTypeConfiguration<ExcelLogDetail>
{
    public void Configure(EntityTypeBuilder<ExcelLogDetail> builder)
    {
        builder.HasOne(e => e.ExcelLog)
            .WithMany(l => l.LogDetails)
            .HasForeignKey(e => e.ExcelLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
