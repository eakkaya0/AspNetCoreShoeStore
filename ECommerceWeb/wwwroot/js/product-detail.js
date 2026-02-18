/* ══════════════════════════════════════════════════════════════
   PRODUCT-DETAIL.JS - Ürün detay sayfası
   Adet seçimi: her beden için 1..stok aralığında dropdown.
══════════════════════════════════════════════════════════════ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var qtySelect = document.getElementById('productQuantity');
        var sizeSelector = document.getElementById('sizeSelector');
        var btnAddToCart = document.getElementById('btnAddToCart');
        var qtyInfo = document.getElementById('quantityInfo');

        // Görsel galeri: thumbnail tıklanınca ana görseli değiştir
        var mainProductImage = document.getElementById('mainProductImage');
        var thumbButtons = document.querySelectorAll('.product-thumb');
        if (mainProductImage && thumbButtons && thumbButtons.length > 0) {
            thumbButtons.forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var src = (this.getAttribute('data-src') || '').trim();
                    if (!src) return;

                    mainProductImage.setAttribute('src', src);

                    thumbButtons.forEach(function (b) {
                        b.classList.remove('active');
                    });
                    this.classList.add('active');
                });
            });
        }

        if (!qtySelect) return;

        function updateInfo() {
            if (!qtyInfo || qtySelect.disabled) return;
            var q = parseInt(qtySelect.value || '1', 10);
            if (isNaN(q) || q < 1) q = 1;
            qtyInfo.textContent = 'Seçilen: ' + q + ' adet';
        }

        function buildOptions(max) {
            qtySelect.innerHTML = '';
            var safeMax = (isNaN(max) || max < 1) ? 1 : max;
            for (var i = 1; i <= safeMax; i++) {
                var opt = document.createElement('option');
                opt.value = String(i);
                opt.textContent = String(i);
                qtySelect.appendChild(opt);
            }
            qtySelect.value = '1';
            updateInfo();
        }

        // Beden varsa: önce beden seçilsin, sonra stok kadar adet gösterilsin
        if (sizeSelector) {
            qtySelect.disabled = true;
            sizeSelector.addEventListener('click', function (e) {
                var btn = e.target.closest('.size-btn');
                if (!btn || btn.disabled) return;

                var stockVal = (btn.getAttribute('data-stock') || '').trim();
                var stock = stockVal ? parseInt(stockVal, 10) : 0;
                if (isNaN(stock) || stock <= 0) {
                    qtySelect.disabled = true;
                    qtySelect.innerHTML = '<option value=\"1\">1</option>';
                    updateInfo();
                    return;
                }

                qtySelect.disabled = false;
                buildOptions(stock);

                // Aktif beden stili
                sizeSelector.querySelectorAll('.size-btn').forEach(function (b) {
                    b.classList.remove('btn-primary', 'active');
                    b.classList.add('btn-outline-secondary');
                });
                btn.classList.remove('btn-outline-secondary');
                btn.classList.add('btn-primary', 'active');
                btn.classList.add('active');

                if (btnAddToCart) btnAddToCart.disabled = false;
            });
        }

        // Seçim değiştiğinde info'yu güncelle
        qtySelect.addEventListener('change', updateInfo);

        // Bedensiz ürünlerde sayfa yüklenince mevcut değeri göster
        if (!sizeSelector) {
            updateInfo();
        }

        // Sepete Ekle butonu: seçili adet dropdown'dan okunur
        if (btnAddToCart) {
            btnAddToCart.addEventListener('click', function () {
                var productId = this.getAttribute('data-product-id');
                var productName = this.getAttribute('data-product-name');
                var quantity = parseInt(qtySelect.value || '1', 10);
                if (isNaN(quantity) || quantity < 1) quantity = 1;

                var productVariantId = null;
                
                // Beden seçimi kontrolü
                if (sizeSelector) {
                    var activeSize = sizeSelector.querySelector('.size-btn.active, .size-btn.btn-primary');
                    if (!activeSize) {
                        if (typeof showToast === 'function') showToast('Lütfen beden seçin.', 'warning');
                        return;
                    }
                    productVariantId = activeSize.getAttribute('data-variant-id');
                    if (productVariantId === '0' || productVariantId === null) {
                        productVariantId = null;
                    } else {
                        productVariantId = parseInt(productVariantId);
                    }
                }

                // Dropdown zaten 1..stok ürettiği için burada ekstra sınır gerekmez; yine de güvenlik için:
                var maxAllowed = qtySelect.options.length;
                if (quantity > maxAllowed) quantity = maxAllowed;

                // AJAX ile sepete ekle
                addToCart(productId, productVariantId, quantity, productName);
            });
        }

        // Sepete ekleme fonksiyonu
        function addToCart(productId, productVariantId, quantity, productName) {
            // Loading durumunu göster
            if (btnAddToCart) {
                btnAddToCart.disabled = true;
                var originalText = btnAddToCart.innerHTML;
                btnAddToCart.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Ekleniyor...';
            }

            var data = {
                productId: productId,
                productVariantId: productVariantId,
                quantity: quantity
            };

            // CSRF token'ını al
            var token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                data.__RequestVerificationToken = token.value;
            }

            fetch('/ShoppingCart/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token ? token.value : ''
                },
                body: JSON.stringify(data)
            })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    var msg = '"' + productName + '" ' + quantity + ' adet sepete eklendi!';
                    if (typeof showToast === 'function') showToast(msg, 'success');
                    
                    // Sepet sayısını güncelle
                    updateCartCount(result.cartCount);
                } else {
                    if (typeof showToast === 'function') showToast(result.message || 'Bir hata oluştu.', 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                if (typeof showToast === 'function') showToast('Bir hata oluştu.', 'error');
            })
            .finally(() => {
                // Butonu eski haline getir
                if (btnAddToCart) {
                    btnAddToCart.disabled = false;
                    btnAddToCart.innerHTML = originalText;
                }
            });
        }

        // Sepet sayısını güncelleme fonksiyonu
        function updateCartCount(count) {
            // Header'daki sepet sayısını güncelle
            var cartCountElements = document.querySelectorAll('.cart-count');
            cartCountElements.forEach(function(element) {
                element.textContent = count;
                if (count > 0) {
                    element.style.display = 'inline-block';
                } else {
                    element.style.display = 'none';
                }
            });

            // Sepet badge varsa onu da güncelle
            var cartBadges = document.querySelectorAll('.cart-badge');
            cartBadges.forEach(function(badge) {
                badge.textContent = count;
                if (count > 0) {
                    badge.classList.remove('d-none');
                } else {
                    badge.classList.add('d-none');
                }
            });
        }
    });
})();
