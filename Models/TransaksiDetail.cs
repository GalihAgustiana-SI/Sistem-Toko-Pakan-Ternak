using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemTokoPakan.Models;

public class TransaksiDetail
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TransaksiId { get; set; }

    [Required]
    public int ProdukId { get; set; }

    [Required]
    [StringLength(100)]
    public string NamaPakan { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal HargaSatuan { get; set; }

    [Required]
    public int Jumlah { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [ForeignKey("TransaksiId")]
    public Transaksi? Transaksi { get; set; }

    [ForeignKey("ProdukId")]
    public Produk? Produk { get; set; }
}
