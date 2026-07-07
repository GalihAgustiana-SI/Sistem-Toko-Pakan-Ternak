using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemTokoPakan.Data;

namespace SistemTokoPakan.Controllers;

public class LaporanController : Controller
{
    private readonly AppDbContext _context;

    public LaporanController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? mulai, DateTime? sampai)
    {
        // Default to today if no dates provided
        var startDate = mulai ?? DateTime.Today;
        var endDate = sampai ?? DateTime.Today;

        // Include end of day
        var endDateInclusive = endDate.AddDays(1);

        var transaksi = await _context.Transaksi
            .Include(t => t.Details)
            .Where(t => t.Tanggal >= startDate && t.Tanggal < endDateInclusive)
            .OrderByDescending(t => t.Tanggal)
            .ToListAsync();

        var totalPendapatan = transaksi.Sum(t => t.TotalHarga);

        ViewBag.Mulai = startDate.ToString("yyyy-MM-dd");
        ViewBag.Sampai = endDate.ToString("yyyy-MM-dd");
        ViewBag.TotalPendapatan = totalPendapatan;
        ViewBag.TotalTransaksi = transaksi.Count;

        return View(transaksi);
    }
}
