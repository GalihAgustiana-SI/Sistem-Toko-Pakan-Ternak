using Microsoft.EntityFrameworkCore;
using SistemTokoPakan.Models;

namespace SistemTokoPakan.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produk> Produk { get; set; }
    public DbSet<Transaksi> Transaksi { get; set; }
    public DbSet<TransaksiDetail> TransaksiDetail { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<TransaksiDetail>()
            .HasOne(d => d.Transaksi)
            .WithMany(t => t.Details)
            .HasForeignKey(d => d.TransaksiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TransaksiDetail>()
            .HasOne(d => d.Produk)
            .WithMany()
            .HasForeignKey(d => d.ProdukId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed sample feed products
        modelBuilder.Entity<Produk>().HasData(
            new Produk { Id = 1, SKU = "PKN-001", NamaPakan = "Pur Babi 50kg", Kategori = "Babi", Stok = 25, HargaJual = 385000m, CreatedAt = new DateTime(2026, 1, 1) },
            new Produk { Id = 2, SKU = "PKN-002", NamaPakan = "Konsentrat Sapi Perah", Kategori = "Sapi", Stok = 18, HargaJual = 290000m, CreatedAt = new DateTime(2026, 1, 1) },
            new Produk { Id = 3, SKU = "PKN-003", NamaPakan = "Pakan Ayam Broiler Starter", Kategori = "Ayam", Stok = 40, HargaJual = 345000m, CreatedAt = new DateTime(2026, 1, 1) },
            new Produk { Id = 4, SKU = "PKN-004", NamaPakan = "Pakan Ayam Layer", Kategori = "Ayam", Stok = 3, HargaJual = 320000m, CreatedAt = new DateTime(2026, 1, 1) },
            new Produk { Id = 5, SKU = "PKN-005", NamaPakan = "Pakan Ikan Lele", Kategori = "Ikan", Stok = 30, HargaJual = 195000m, CreatedAt = new DateTime(2026, 1, 1) },
            new Produk { Id = 6, SKU = "PKN-006", NamaPakan = "Dedak Halus", Kategori = "Umum", Stok = 2, HargaJual = 125000m, CreatedAt = new DateTime(2026, 1, 1) },
            new Produk { Id = 7, SKU = "PKN-007", NamaPakan = "Jagung Giling 50kg", Kategori = "Umum", Stok = 15, HargaJual = 210000m, CreatedAt = new DateTime(2026, 1, 1) },
            new Produk { Id = 8, SKU = "PKN-008", NamaPakan = "Konsentrat Kambing", Kategori = "Kambing", Stok = 12, HargaJual = 265000m, CreatedAt = new DateTime(2026, 1, 1) }
        );
    }
}
