using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemTokoPakan.Data;
using SistemTokoPakan.Models;

namespace SistemTokoPakan.Controllers;

public class InventoryController : Controller
{
    private readonly AppDbContext _context;

    public InventoryController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Produk.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.NamaPakan.ToLower().Contains(search) ||
                p.SKU.ToLower().Contains(search) ||
                p.Kategori.ToLower().Contains(search));
        }

        var produk = await query.OrderBy(p => p.NamaPakan).ToListAsync();
        ViewBag.Search = search;
        return View(produk);
    }

    [HttpGet]
    public async Task<IActionResult> GetProduk(int id)
    {
        var produk = await _context.Produk.FindAsync(id);
        if (produk == null) return NotFound();

        return Json(new
        {
            produk.Id,
            produk.SKU,
            produk.NamaPakan,
            produk.Kategori,
            produk.Stok,
            produk.HargaJual
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Produk produk)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Data tidak valid." });

        // Check duplicate SKU
        var exists = await _context.Produk.AnyAsync(p => p.SKU == produk.SKU);
        if (exists)
            return BadRequest(new { message = "SKU sudah digunakan." });

        produk.CreatedAt = DateTime.Now;
        _context.Produk.Add(produk);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Produk berhasil ditambahkan.", produk.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] Produk produk)
    {
        var existing = await _context.Produk.FindAsync(produk.Id);
        if (existing == null)
            return NotFound(new { message = "Produk tidak ditemukan." });

        // Check duplicate SKU (exclude current)
        var skuExists = await _context.Produk.AnyAsync(p => p.SKU == produk.SKU && p.Id != produk.Id);
        if (skuExists)
            return BadRequest(new { message = "SKU sudah digunakan." });

        existing.SKU = produk.SKU;
        existing.NamaPakan = produk.NamaPakan;
        existing.Kategori = produk.Kategori;
        existing.Stok = produk.Stok;
        existing.HargaJual = produk.HargaJual;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Produk berhasil diperbarui." });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var produk = await _context.Produk.FindAsync(id);
        if (produk == null)
            return NotFound(new { message = "Produk tidak ditemukan." });

        // Check if product is used in any transaction
        var used = await _context.TransaksiDetail.AnyAsync(d => d.ProdukId == id);
        if (used)
            return BadRequest(new { message = "Produk tidak bisa dihapus karena sudah ada di transaksi." });

        _context.Produk.Remove(produk);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Produk berhasil dihapus." });
    }
}
