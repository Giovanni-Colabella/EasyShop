using API.Models.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentCarrelloProdottoConfig : IEntityTypeConfiguration<CarrelloProdotto>
{
    public void Configure(EntityTypeBuilder<CarrelloProdotto> builder)
    {
        builder.HasKey(cp => new { cp.CarrelloId, cp.ProdottoId }); // Chiave primaria composta
        builder
            .HasOne(cp => cp.Carrello)
            .WithMany(c => c.CarrelloProdotti)
            .HasForeignKey(cp => cp.CarrelloId)
            .OnDelete(DeleteBehavior.Cascade); // Se un carrello viene cancellato, cancella anche i suoi prodotti

        builder.HasOne(cp => cp.Prodotto)
            .WithMany(p => p.CarrelloProdotti)
            .HasForeignKey(cp => cp.ProdottoId)
            .OnDelete(DeleteBehavior.Cascade); // Se un prodotto viene cancellato, cancella anche i suoi carrelli

        builder.Property(cp => cp.Quantita).HasDefaultValue<int>(1); // Imposta valore predefinito per la quantita
        
    }
}
