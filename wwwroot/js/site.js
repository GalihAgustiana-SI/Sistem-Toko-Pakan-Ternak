/* ============================================
   HARVEST ORGANIC — Client-Side Logic
   The Farmer's Choice
   ============================================ */

// ---- Cart State ----
let cart = [];

// ---- Toast Notifications ----
function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const iconSvg = type === 'success'
        ? '<svg class="toast-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>'
        : '<svg class="toast-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>';

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `${iconSvg}<span>${message}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('fade-out');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ---- Format Currency ----
function formatRupiah(num) {
    return 'Rp ' + Number(num).toLocaleString('id-ID');
}

// ============================================
// INVENTORY PAGE
// ============================================

// Search functionality
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('inventory-search');
    if (searchInput) {
        let debounceTimer;
        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                window.location.href = '/Inventory?search=' + encodeURIComponent(this.value);
            }, 500);
        });
    }

    // POS search
    const posSearch = document.getElementById('pos-search');
    if (posSearch) {
        posSearch.addEventListener('input', function () {
            const query = this.value.toLowerCase();
            const cards = document.querySelectorAll('.product-card');
            cards.forEach(card => {
                const name = card.querySelector('.product-card-name').textContent.toLowerCase();
                card.style.display = name.includes(query) ? '' : 'none';
            });
        });
    }

    // Payment input change
    const jumlahBayar = document.getElementById('jumlah-bayar');
    if (jumlahBayar) {
        jumlahBayar.addEventListener('input', function () {
            updateChange();
        });
    }
});

// ---- Add Product Modal ----
function openAddModal() {
    document.getElementById('modal-title').textContent = 'Tambah Produk';
    document.getElementById('produk-id').value = '0';
    document.getElementById('produk-sku').value = '';
    document.getElementById('produk-nama').value = '';
    document.getElementById('produk-kategori').value = '';
    document.getElementById('produk-stok').value = '';
    document.getElementById('produk-harga').value = '';
    document.getElementById('product-modal').style.display = 'flex';
}

// ---- Edit Product Modal ----
async function openEditModal(id) {
    try {
        const res = await fetch(`/Inventory/GetProduk?id=${id}`);
        if (!res.ok) throw new Error('Gagal mengambil data produk');
        const data = await res.json();

        document.getElementById('modal-title').textContent = 'Edit Produk';
        document.getElementById('produk-id').value = data.id;
        document.getElementById('produk-sku').value = data.sku;
        document.getElementById('produk-nama').value = data.namaPakan;
        document.getElementById('produk-kategori').value = data.kategori;
        document.getElementById('produk-stok').value = data.stok;
        document.getElementById('produk-harga').value = data.hargaJual;
        document.getElementById('product-modal').style.display = 'flex';
    } catch (err) {
        showToast(err.message, 'error');
    }
}

function closeProductModal() {
    document.getElementById('product-modal').style.display = 'none';
}

// ---- Save Product (Create or Edit) ----
async function saveProduct() {
    const id = parseInt(document.getElementById('produk-id').value);
    const data = {
        id: id,
        sku: document.getElementById('produk-sku').value.trim(),
        namaPakan: document.getElementById('produk-nama').value.trim(),
        kategori: document.getElementById('produk-kategori').value,
        stok: parseInt(document.getElementById('produk-stok').value) || 0,
        hargaJual: parseFloat(document.getElementById('produk-harga').value) || 0
    };

    // Validation
    if (!data.sku || !data.namaPakan || !data.kategori) {
        showToast('Mohon lengkapi semua field.', 'error');
        return;
    }

    const url = id === 0 ? '/Inventory/Create' : '/Inventory/Edit';

    try {
        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await res.json();

        if (res.ok) {
            showToast(result.message);
            closeProductModal();
            setTimeout(() => location.reload(), 500);
        } else {
            showToast(result.message || 'Gagal menyimpan data.', 'error');
        }
    } catch (err) {
        showToast('Gagal menyimpan data.', 'error');
    }
}

// ---- Delete Product ----
function confirmDelete(id, name) {
    document.getElementById('delete-product-id').value = id;
    document.getElementById('delete-product-name').textContent = name;
    document.getElementById('delete-modal').style.display = 'flex';
}

function closeDeleteModal() {
    document.getElementById('delete-modal').style.display = 'none';
}

async function deleteProduct() {
    const id = document.getElementById('delete-product-id').value;

    try {
        const res = await fetch(`/Inventory/Delete?id=${id}`, {
            method: 'POST'
        });

        const result = await res.json();

        if (res.ok) {
            showToast(result.message);
            closeDeleteModal();
            const row = document.getElementById(`row-${id}`);
            if (row) {
                row.style.opacity = '0';
                row.style.transform = 'translateX(-20px)';
                row.style.transition = 'all 0.3s ease';
                setTimeout(() => {
                    row.remove();
                    // Check if table is empty
                    const tbody = document.querySelector('#inventory-table tbody');
                    if (tbody && tbody.children.length === 0) {
                        location.reload();
                    }
                }, 300);
            }
        } else {
            showToast(result.message || 'Gagal menghapus produk.', 'error');
        }
    } catch (err) {
        showToast('Gagal menghapus produk.', 'error');
    }
}

// ============================================
// POS / KASIR PAGE
// ============================================

function addToCart(id, nama, harga, stokMax) {
    // Flash the card
    const card = document.getElementById(`product-card-${id}`);
    if (card) {
        card.classList.remove('flash');
        void card.offsetWidth; // Force reflow
        card.classList.add('flash');
    }

    // Check if item already in cart
    const existing = cart.find(item => item.produkId === id);

    if (existing) {
        if (existing.jumlah >= stokMax) {
            showToast(`Stok ${nama} tidak mencukupi`, 'error');
            return;
        }
        existing.jumlah++;
        existing.subtotal = existing.jumlah * existing.hargaSatuan;
    } else {
        cart.push({
            produkId: id,
            namaPakan: nama,
            hargaSatuan: harga,
            jumlah: 1,
            subtotal: harga,
            stokMax: stokMax
        });
    }

    renderCart();
}

function updateQty(produkId, delta) {
    const item = cart.find(i => i.produkId === produkId);
    if (!item) return;

    const newQty = item.jumlah + delta;
    if (newQty <= 0) {
        removeFromCart(produkId);
        return;
    }
    if (newQty > item.stokMax) {
        showToast(`Stok ${item.namaPakan} tidak mencukupi`, 'error');
        return;
    }

    item.jumlah = newQty;
    item.subtotal = item.jumlah * item.hargaSatuan;
    renderCart();
}

function removeFromCart(produkId) {
    cart = cart.filter(i => i.produkId !== produkId);
    renderCart();
}

function clearCart() {
    cart = [];
    renderCart();
}

function renderCart() {
    const container = document.getElementById('cart-items');
    const footer = document.getElementById('cart-footer');
    const emptyState = document.getElementById('cart-empty');
    const clearBtn = document.getElementById('btn-clear-cart');
    const totalValue = document.getElementById('cart-total-value');

    if (!container) return;

    // Clear existing items (except empty state)
    const existingItems = container.querySelectorAll('.cart-item');
    existingItems.forEach(el => el.remove());

    if (cart.length === 0) {
        if (emptyState) emptyState.style.display = '';
        if (footer) footer.style.display = 'none';
        if (clearBtn) clearBtn.style.display = 'none';
        return;
    }

    if (emptyState) emptyState.style.display = 'none';
    if (footer) footer.style.display = '';
    if (clearBtn) clearBtn.style.display = '';

    let total = 0;

    cart.forEach(item => {
        total += item.subtotal;

        const div = document.createElement('div');
        div.className = 'cart-item';
        div.innerHTML = `
            <div class="cart-item-info">
                <div class="cart-item-name">${item.namaPakan}</div>
                <div class="cart-item-price">${formatRupiah(item.hargaSatuan)}</div>
            </div>
            <div class="qty-stepper">
                <button onclick="updateQty(${item.produkId}, -1)">−</button>
                <span class="qty-value">${item.jumlah}</span>
                <button onclick="updateQty(${item.produkId}, 1)">+</button>
            </div>
            <span class="cart-item-subtotal">${formatRupiah(item.subtotal)}</span>
            <button class="cart-item-remove" onclick="removeFromCart(${item.produkId})">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
            </button>
        `;
        container.appendChild(div);
    });

    if (totalValue) totalValue.textContent = formatRupiah(total);
}

// ---- Payment Modal ----
function openPaymentModal() {
    if (cart.length === 0) {
        showToast('Keranjang masih kosong.', 'error');
        return;
    }

    const total = cart.reduce((sum, item) => sum + item.subtotal, 0);
    document.getElementById('payment-total').textContent = formatRupiah(total);
    document.getElementById('jumlah-bayar').value = '';
    document.getElementById('payment-change-section').style.display = 'none';
    document.getElementById('payment-modal').style.display = 'flex';

    // Auto-focus on cash input
    setTimeout(() => {
        document.getElementById('jumlah-bayar').focus();
    }, 100);
}

function closePaymentModal() {
    document.getElementById('payment-modal').style.display = 'none';
}

function updateChange() {
    const total = cart.reduce((sum, item) => sum + item.subtotal, 0);
    const bayar = parseFloat(document.getElementById('jumlah-bayar').value) || 0;
    const changeSection = document.getElementById('payment-change-section');
    const kembalianEl = document.getElementById('payment-kembalian');

    if (bayar >= total) {
        changeSection.style.display = '';
        kembalianEl.textContent = formatRupiah(bayar - total);
    } else {
        changeSection.style.display = 'none';
    }
}

async function processCheckout() {
    const total = cart.reduce((sum, item) => sum + item.subtotal, 0);
    const jumlahBayar = parseFloat(document.getElementById('jumlah-bayar').value) || 0;

    if (jumlahBayar < total) {
        showToast('Jumlah bayar kurang dari total belanja.', 'error');
        return;
    }

    const requestData = {
        jumlahBayar: jumlahBayar,
        items: cart.map(item => ({
            produkId: item.produkId,
            namaPakan: item.namaPakan,
            hargaSatuan: item.hargaSatuan,
            jumlah: item.jumlah,
            subtotal: item.subtotal
        }))
    };

    try {
        const res = await fetch('/Kasir/Checkout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(requestData)
        });

        const result = await res.json();

        if (res.ok) {
            showToast('Transaksi berhasil!');
            closePaymentModal();

            // Populate receipt
            populateReceipt(result);

            // Print receipt
            printReceipt();

            // Clear cart and reload
            cart = [];
            renderCart();

            // Reload page after print to refresh stock
            setTimeout(() => location.reload(), 1000);
        } else {
            showToast(result.message || 'Gagal memproses transaksi.', 'error');
        }
    } catch (err) {
        showToast('Gagal memproses transaksi.', 'error');
    }
}

function populateReceipt(data) {
    document.getElementById('receipt-no').textContent = data.noStruk;
    document.getElementById('receipt-date').textContent = data.tanggal;
    document.getElementById('receipt-total').textContent = formatRupiah(data.totalHarga);
    document.getElementById('receipt-bayar').textContent = formatRupiah(data.jumlahBayar);
    document.getElementById('receipt-kembali').textContent = formatRupiah(data.kembalian);

    const tbody = document.querySelector('#receipt-items tbody');
    tbody.innerHTML = '';
    data.items.forEach(item => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td style="text-align:left;">${item.namaPakan}</td>
            <td style="text-align:center;">${item.jumlah}</td>
            <td style="text-align:right;">${formatRupiah(item.subtotal)}</td>
        `;
        tbody.appendChild(tr);
    });
}

function printReceipt() {
    const receiptTemplate = document.getElementById('receipt-template');
    if (!receiptTemplate) return;

    const printWindow = window.open('', '_blank', 'width=320,height=600');
    printWindow.document.write(`
        <html>
        <head>
            <title>Struk Pembelian</title>
            <style>
                body { font-family: 'Courier New', monospace; font-size: 12px; margin: 0; padding: 10px; }
                .receipt { width: 280px; margin: 0 auto; }
                .receipt-header { text-align: center; margin-bottom: 8px; }
                .receipt-brand { font-size: 18px; font-weight: 700; }
                .receipt-tagline { font-size: 10px; color: #666; }
                .receipt-divider { border-top: 1px dashed #333; margin: 8px 0; }
                .receipt-info p { margin: 2px 0; font-size: 11px; }
                .receipt-table { width: 100%; border-collapse: collapse; font-size: 11px; }
                .receipt-table th, .receipt-table td { padding: 2px 0; }
                .receipt-totals p { margin: 2px 0; }
                .receipt-footer { text-align: center; font-size: 10px; color: #666; margin-top: 8px; }
            </style>
        </head>
        <body>
            ${document.getElementById('receipt-content').innerHTML}
            <script>window.onload = function() { window.print(); window.close(); }<\/script>
        </body>
        </html>
    `);
    printWindow.document.close();
}

// ============================================
// REPORTS PAGE
// ============================================

function filterLaporan() {
    const mulai = document.getElementById('filter-mulai').value;
    const sampai = document.getElementById('filter-sampai').value;

    if (!mulai || !sampai) {
        showToast('Mohon pilih rentang tanggal.', 'error');
        return;
    }

    window.location.href = `/Laporan?mulai=${mulai}&sampai=${sampai}`;
}

// ---- Close modals on overlay click ----
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-overlay')) {
        e.target.style.display = 'none';
    }
});

// ---- Close modals on Escape key ----
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const modals = document.querySelectorAll('.modal-overlay');
        modals.forEach(modal => {
            if (modal.style.display !== 'none') {
                modal.style.display = 'none';
            }
        });
    }
});
