using API.Models;
using API.Models.Entities;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // esegui uno scan dell'assembly e applica tutte le configurazioni Fluent API
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        }

        // Tabelle 
        #region  Tabelle 
        public DbSet<Cliente> Clienti { get; set; } = null!; // Tabella Clienti
        public DbSet<Ordine> Ordini { get; set; } = null!; // Tabella Ordini
        public DbSet<Prodotto> Prodotti { get; set; } = null!; // Tabella Prodotti
        public DbSet<DettaglioOrdine> DettagliOrdini { get; set; } = null!; // Tabella intermedia DettagliOrdini 
        public DbSet<BannedIp> BannedIps { get; set; } = null!; // Tabella BannedIps 
        public DbSet<Carrello> Carrelli { get; set; } = null!; // Tabella carrelli 
        public DbSet<CarrelloProdotto> CarrelloProdotti { get; set; } = null!; // Tabella intermedia CarrelloProdotti
        public DbSet<ProfilePicture> ProfilePictures { get; set; } = null!; // Tabella immagini profilo
        public DbSet<ExcelMappingHeader> ExcelMappingHeaders { get; set; }
        public DbSet<ExcelMappingDetail> ExcelMappingDetails { get; set; }
        public DbSet<ExcelLog> ExcelLogs { get; set; }
        public DbSet<ExcelLogDetail> ExcelLogDetails { get; set; }
        #endregion
    }
}
 