using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemTokoPakan.Models;

public class Transaksi
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string NoStruk { get; set; } = string.Empty;

    [Required]
    public DateTime Tanggal { get; set; } = DateTime.Now;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalHarga { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal JumlahBayar { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Kembalian { get; set; }

    public List<TransaksiDetail> Details { get; set; } = new();
}
