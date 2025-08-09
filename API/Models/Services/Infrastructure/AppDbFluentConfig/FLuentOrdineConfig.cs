using System;
using API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentOrdineConfig : IEntityTypeConfiguration<Ordine>
{
    public void Configure(EntityTypeBuilder<Ordine> builder)
    {
        builder.HasOne(o => o.Cliente)
            .WithMany(c => c.Ordini)
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Cascade); // Se un cliente viene cancellato, cancella anche i suoi ordini

        builder.Property(o => o.TotaleOrdine)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.Stato)
            .HasConversion<string>() // Converte lo stato in stringa per il database
            .IsRequired();

        builder.Property(o => o.MetodoPagamento)
            .IsRequired();
    }
}
