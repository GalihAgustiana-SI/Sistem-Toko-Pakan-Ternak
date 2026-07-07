using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemTokoPakan.Models;

public class Produk
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string NamaPakan { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Kategori { get; set; } = string.Empty;

    [Required]
    public int Stok { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal HargaJual { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
