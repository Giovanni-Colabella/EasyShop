using API.Models.Entities;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models.Services.Infrastructure.AppDbFluentConfig;

public class FluentAppUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.Status).HasDefaultValue(ApplicationUserStatus.Attivo);
        builder.Property(u => u.Status).HasConversion<string>();
        
        builder.HasOne(u => u.Cliente)
            .WithOne(c => c.User)
            .HasForeignKey<Cliente>(c => c.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Filtro globale
        builder.HasQueryFilter(user => user.Status != ApplicationUserStatus.Eliminato &&
            user.Status != ApplicationUserStatus.Bannato);  
    
    }
}
