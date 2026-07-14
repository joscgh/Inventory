using Inventory.Core;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Esta línea le dice a Entity Framework que cree la tabla "Items" basada en nuestra clase
        public DbSet<ItemUniversal> Items { get; set; }
        public DbSet<Inventory.Core.Classes.Attribute> Attributes { get; set; }
        public DbSet<Inventory.Core.Classes.Currency> Currencies { get; set; }
        public DbSet<Inventory.Core.Classes.Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la clave primaria de ItemUniversal
            modelBuilder.Entity<ItemUniversal>().HasKey(i => i.Id);

            modelBuilder.Entity<ItemUniversal>()
                .HasOne(i => i.Currency)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ItemUniversal>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar el mapeo de los atributos dinámicos
            modelBuilder.Entity<ItemUniversal>()
                .HasMany(i => i.Attributes)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade); // Borrado en cascada

            modelBuilder.Entity<Inventory.Core.Classes.Attribute>()
                .HasKey(a => new { a.Name, a.Value });

            modelBuilder.Entity<Inventory.Core.Classes.Currency>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Inventory.Core.Classes.Currency>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Inventory.Core.Classes.Category>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Inventory.Core.Classes.Category>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }
    }
}
