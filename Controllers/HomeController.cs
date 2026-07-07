using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemTokoPakan.Data;
using SistemTokoPakan.Models;

namespace SistemTokoPakan.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Today's revenue
        var pendapatanHariIni = await _context.Transaksi
            .Where(t => t.Tanggal >= today && t.Tanggal < tomorrow)
            .SumAsync(t => t.TotalHarga);

        // Today's transaction count
        var totalTransaksiHariIni = await _context.Transaksi
            .Where(t => t.Tanggal >= today && t.Tanggal < tomorrow)
            .CountAsync();

        // Total products
        var totalProduk = await _context.Produk.CountAsync();

        // Low stock items (under 5 sacks)
        var stokRendah = await _context.Produk
            .Where(p => p.Stok < 5)
            .OrderBy(p => p.Stok)
            .ToListAsync();

        // Recent transactions (last 10)
        var transaksiTerakhir = await _context.Transaksi
            .Include(t => t.Details)
            .OrderByDescending(t => t.Tanggal)
            .Take(10)
            .ToListAsync();

        ViewBag.PendapatanHariIni = pendapatanHariIni;
        ViewBag.TotalTransaksiHariIni = totalTransaksiHariIni;
        ViewBag.TotalProduk = totalProduk;
        ViewBag.StokRendah = stokRendah;
        ViewBag.TransaksiTerakhir = transaksiTerakhir;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
