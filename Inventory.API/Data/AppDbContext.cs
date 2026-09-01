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
        public DbSet<Inventory.Core.Classes.InventoryAdjustment> Adjustments { get; set; }
        public DbSet<Inventory.Core.Classes.PriceVariant> PriceVariants { get; set; }
        public DbSet<Inventory.Core.Classes.Attribute> Attributes { get; set; }
        public DbSet<Inventory.Core.Classes.Currency> Currencies { get; set; }
        public DbSet<Inventory.Core.Classes.Category> Categories { get; set; }
        public DbSet<Inventory.Core.Classes.Tax> Taxes { get; set; }
        public DbSet<Inventory.Core.Classes.Note> Notes { get; set; }
        public DbSet<Inventory.Core.Classes.NoteLine> NoteLines { get; set; }
        public DbSet<Inventory.Core.Classes.CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<Inventory.Core.Classes.CustomerAccountUser> CustomerAccountUsers { get; set; }
        public DbSet<Inventory.Core.Classes.ConsumerCustomer> ConsumerCustomers { get; set; }
        public DbSet<Inventory.Core.Classes.AccountLocation> AccountLocations { get; set; }
        public DbSet<Inventory.Core.Classes.AccountLogo> AccountLogos { get; set; }
        public DbSet<Inventory.Core.Classes.ItemStock> ItemStocks { get; set; }
        public DbSet<Inventory.Core.Classes.Terminal> Terminals { get; set; }
        public DbSet<Inventory.Core.Classes.InvoiceNumberRange> InvoiceNumberRanges { get; set; }
        public DbSet<Inventory.Core.Classes.Invoice> Invoices { get; set; }
        public DbSet<Inventory.Core.Classes.InvoiceLine> InvoiceLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la clave primaria de ItemUniversal
            modelBuilder.Entity<ItemUniversal>().HasKey(i => i.Id);

            // Índice para buscar rápido por código de barras al escanear.
            modelBuilder.Entity<ItemUniversal>()
                .HasIndex(i => i.Barcode);

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

            modelBuilder.Entity<ItemUniversal>()
                .HasMany(i => i.Taxes)
                .WithMany(t => t.Items);

            modelBuilder.Entity<Inventory.Core.Classes.Attribute>()
                .HasKey(a => new { a.Name, a.Value });

            modelBuilder.Entity<Inventory.Core.Classes.Tax>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Inventory.Core.Classes.Tax>()
                .HasIndex(t => t.Name)
                .IsUnique();

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

            modelBuilder.Entity<Inventory.Core.Classes.InventoryAdjustment>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Inventory.Core.Classes.InventoryAdjustment>()
                .HasOne(a => a.Item)
                .WithMany()
                .HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // El historial conserva el depósito aunque éste se borre después.
            modelBuilder.Entity<Inventory.Core.Classes.InventoryAdjustment>()
                .HasOne(a => a.Location)
                .WithMany()
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Existencias por depósito / tienda.
            modelBuilder.Entity<Inventory.Core.Classes.ItemStock>()
                .HasKey(s => new { s.ItemId, s.LocationId });

            modelBuilder.Entity<Inventory.Core.Classes.ItemStock>()
                .HasOne(s => s.Item)
                .WithMany(i => i.StockByLocation)
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.ItemStock>()
                .HasOne(s => s.Location)
                .WithMany()
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.PriceVariant>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Inventory.Core.Classes.PriceVariant>()
                .HasOne(p => p.Item)
                .WithMany(i => i.PriceVariants)
                .HasForeignKey(p => p.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasOne(n => n.CustomerAccount)
                .WithMany()
                .HasForeignKey(n => n.CustomerAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasOne(n => n.ConsumerCustomer)
                .WithMany(c => c.Notes)
                .HasForeignKey(n => n.ConsumerCustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict: una nota emitida deja constancia de su depósito y tienda,
            // así que no se permite borrar una ubicación que ya tiene notas.
            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasOne(n => n.Warehouse)
                .WithMany()
                .HasForeignKey(n => n.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasOne(n => n.Store)
                .WithMany()
                .HasForeignKey(n => n.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasOne(n => n.CreatedByUser)
                .WithMany()
                .HasForeignKey(n => n.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasOne(n => n.Currency)
                .WithMany()
                .HasForeignKey(n => n.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasOne(n => n.ReferenceNote)
                .WithMany(n => n.ReferencedByNotes)
                .HasForeignKey(n => n.ReferenceNoteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Note>()
                .HasMany(n => n.Lines)
                .WithOne(l => l.Note)
                .HasForeignKey(l => l.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.NoteLine>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Inventory.Core.Classes.NoteLine>()
                .HasOne(l => l.Item)
                .WithMany()
                .HasForeignKey(l => l.ItemUniversalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.NoteLine>()
                .HasOne(l => l.Category)
                .WithMany()
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.NoteLine>()
                .HasOne(l => l.Currency)
                .WithMany()
                .HasForeignKey(l => l.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.CustomerAccount>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Inventory.Core.Classes.CustomerAccount>()
                .HasMany(a => a.Users)
                .WithOne(u => u.Account)
                .HasForeignKey(u => u.CustomerAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.CustomerAccount>()
                .HasMany(a => a.Locations)
                .WithOne(l => l.Account)
                .HasForeignKey(l => l.CustomerAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.AccountLocation>()
                .HasKey(l => l.Id);

            // Dentro de una misma cuenta no se repite el nombre de un depósito o tienda.
            modelBuilder.Entity<Inventory.Core.Classes.AccountLocation>()
                .HasIndex(l => new { l.CustomerAccountId, l.Name })
                .IsUnique();

            // HasLogo se calcula al listar, no es una columna.
            modelBuilder.Entity<Inventory.Core.Classes.CustomerAccount>()
                .Ignore(a => a.HasLogo);

            modelBuilder.Entity<Inventory.Core.Classes.AccountLogo>()
                .HasKey(l => l.CustomerAccountId);

            modelBuilder.Entity<Inventory.Core.Classes.AccountLogo>()
                .HasOne(l => l.Account)
                .WithOne()
                .HasForeignKey<Inventory.Core.Classes.AccountLogo>(l => l.CustomerAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.CustomerAccountUser>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Inventory.Core.Classes.CustomerAccountUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Inventory.Core.Classes.ConsumerCustomer>()
                .HasKey(c => c.Id);

            // ---------------------------------------------------------------
            // Facturación fiscal
            // ---------------------------------------------------------------

            modelBuilder.Entity<Inventory.Core.Classes.Terminal>()
                .HasKey(t => t.Id);

            // Dentro de una cuenta, ni el código ni la serie se repiten entre cajas:
            // dos terminales con la misma serie romperían la unicidad del correlativo.
            modelBuilder.Entity<Inventory.Core.Classes.Terminal>()
                .HasIndex(t => new { t.CustomerAccountId, t.Code })
                .IsUnique();

            modelBuilder.Entity<Inventory.Core.Classes.Terminal>()
                .HasIndex(t => new { t.CustomerAccountId, t.Serie })
                .IsUnique();

            modelBuilder.Entity<Inventory.Core.Classes.Terminal>()
                .HasOne(t => t.Account)
                .WithMany()
                .HasForeignKey(t => t.CustomerAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Terminal>()
                .HasOne(t => t.Store)
                .WithMany()
                .HasForeignKey(t => t.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.InvoiceNumberRange>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Inventory.Core.Classes.InvoiceNumberRange>()
                .HasOne(r => r.Terminal)
                .WithMany(t => t.Ranges)
                .HasForeignKey(r => r.TerminalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Una misma caja no puede recibir dos veces el mismo bloque.
            modelBuilder.Entity<Inventory.Core.Classes.InvoiceNumberRange>()
                .HasIndex(r => new { r.TerminalId, r.DocumentType, r.FromNumber })
                .IsUnique();

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasKey(i => i.Id);

            // La red de seguridad de todo el esquema offline: aunque una caja se
            // equivoque o se restaure un respaldo viejo, la base rechaza un
            // correlativo repetido dentro de la misma serie.
            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasIndex(i => new { i.CustomerAccountId, i.DocumentType, i.Serie, i.Number })
                .IsUnique();

            // Idempotencia de la sincronización: el reintento del POS no duplica.
            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasIndex(i => i.ClientGuid)
                .IsUnique();

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.Terminal)
                .WithMany()
                .HasForeignKey(i => i.TerminalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.NumberRange)
                .WithMany()
                .HasForeignKey(i => i.InvoiceNumberRangeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.CustomerAccount)
                .WithMany()
                .HasForeignKey(i => i.CustomerAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.Store)
                .WithMany()
                .HasForeignKey(i => i.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.Warehouse)
                .WithMany()
                .HasForeignKey(i => i.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.ConsumerCustomer)
                .WithMany()
                .HasForeignKey(i => i.ConsumerCustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.Currency)
                .WithMany()
                .HasForeignKey(i => i.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasOne(i => i.ReferenceInvoice)
                .WithMany(i => i.ReferencedByInvoices)
                .HasForeignKey(i => i.ReferenceInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.Invoice>()
                .HasMany(i => i.Lines)
                .WithOne(l => l.Invoice)
                .HasForeignKey(l => l.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory.Core.Classes.InvoiceLine>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Inventory.Core.Classes.InvoiceLine>()
                .HasOne(l => l.Item)
                .WithMany()
                .HasForeignKey(l => l.ItemUniversalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.InvoiceLine>()
                .HasOne(l => l.Category)
                .WithMany()
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory.Core.Classes.InvoiceLine>()
                .HasOne(l => l.Currency)
                .WithMany()
                .HasForeignKey(l => l.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
