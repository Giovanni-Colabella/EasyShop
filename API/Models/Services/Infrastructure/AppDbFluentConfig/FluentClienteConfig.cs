using System;
using API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentClienteConfig : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.OwnsOne(c => c.Indirizzo, i => {
            i.Property(i => i.Via)
                .HasColumnName("Indirizzo_Via")
                .IsRequired(); // Proprietà obbligatoria 

            i.Property(i => i.Citta)
                .HasColumnName("Indirizzo_Citta")
                .IsRequired();

            i.Property(i => i.CAP)
                .HasColumnName("Indirizzo_CAP")
                .IsRequired();
        });

    }
}

