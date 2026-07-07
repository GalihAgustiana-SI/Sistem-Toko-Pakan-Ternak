using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemTokoPakan.Data;
using SistemTokoPakan.Models;

namespace SistemTokoPakan.Controllers;

public class KasirController : Controller
{
    private readonly AppDbContext _context;

    public KasirController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var produk = await _context.Produk
            .Where(p => p.Stok > 0)
            .OrderBy(p => p.NamaPakan)
            .ToListAsync();

        return View(produk);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            return BadRequest(new { message = "Keranjang kosong." });

        if (request.JumlahBayar < request.Items.Sum(i => i.Subtotal))
            return BadRequest(new { message = "Jumlah bayar kurang." });

        // Validate stock
        foreach (var item in request.Items)
        {
            var produk = await _context.Produk.FindAsync(item.ProdukId);
            if (produk == null)
                return BadRequest(new { message = $"Produk dengan ID {item.ProdukId} tidak ditemukan." });
            if (produk.Stok < item.Jumlah)
                return BadRequest(new { message = $"Stok {produk.NamaPakan} tidak mencukupi. Sisa: {produk.Stok} karung." });
        }

        // Generate receipt number
        var today = DateTime.Today;
        var countToday = await _context.Transaksi
            .Where(t => t.Tanggal >= today && t.Tanggal < today.AddDays(1))
            .CountAsync();

        var noStruk = $"STR-{today:yyyyMMdd}-{(countToday + 1):D4}";

        var totalHarga = request.Items.Sum(i => i.Subtotal);
        var kembalian = request.JumlahBayar - totalHarga;

        // Create transaction
        var transaksi = new Transaksi
        {
            NoStruk = noStruk,
            Tanggal = DateTime.Now,
            TotalHarga = totalHarga,
            JumlahBayar = request.JumlahBayar,
            Kembalian = kembalian
        };

        _context.Transaksi.Add(transaksi);
        await _context.SaveChangesAsync();

        // Create details and deduct stock
        foreach (var item in request.Items)
        {
            var produk = await _context.Produk.FindAsync(item.ProdukId);
            produk!.Stok -= item.Jumlah;

            var detail = new TransaksiDetail
            {
                TransaksiId = transaksi.Id,
                ProdukId = item.ProdukId,
                NamaPakan = item.NamaPakan,
                HargaSatuan = item.HargaSatuan,
                Jumlah = item.Jumlah,
                Subtotal = item.Subtotal
            };

            _context.TransaksiDetail.Add(detail);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Transaksi berhasil.",
            noStruk,
            tanggal = transaksi.Tanggal.ToString("dd/MM/yyyy HH:mm"),
            totalHarga,
            jumlahBayar = request.JumlahBayar,
            kembalian,
            items = request.Items
        });
    }
}

public class CheckoutRequest
{
    public decimal JumlahBayar { get; set; }
    public List<CheckoutItem> Items { get; set; } = new();
}

public class CheckoutItem
{
    public int ProdukId { get; set; }
    public string NamaPakan { get; set; } = string.Empty;
    public decimal HargaSatuan { get; set; }
    public int Jumlah { get; set; }
    public decimal Subtotal { get; set; }
}
