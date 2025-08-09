using API.Models.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentDettaglioOrdineConfig : IEntityTypeConfiguration<DettaglioOrdine>
{
    public void Configure(EntityTypeBuilder<DettaglioOrdine> builder)
    {
        builder.HasOne(d => d.Ordine)
            .WithMany(o => o.DettagliOrdini)
            .HasForeignKey(d => d.OrdineId)
            .OnDelete(DeleteBehavior.Cascade); // Se un ordine viene cancellato, cancella anche i suoi dettagli

        builder.HasOne(d => d.Prodotto)
            .WithMany(p => p.DettagliOrdini)
            .HasForeignKey(d => d.ProdottoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.Quantita)
            .IsRequired();
    }
}
