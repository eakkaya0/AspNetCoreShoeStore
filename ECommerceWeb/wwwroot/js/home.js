/* ══════════════════════════════════════════════════════════════
   HOME.JS - ANA SAYFA JAVASCRIPT
   StepIn E-Ticaret | Modern İnteraktif Özellikler
══════════════════════════════════════════════════════════════ */

(function() {
    'use strict';

    /* ══════════════════════════════════════════════════════════════
       SAYFA YÜKLENDİĞİNDE
    ══════════════════════════════════════════════════════════════ */
    document.addEventListener('DOMContentLoaded', function() {

        const params = new URLSearchParams(window.location.search || '');
        const hasSearchOrFilter =
            (params.get('searchTerm') && params.get('searchTerm').trim().length > 0) ||
            params.has('mainCategoryId') ||
            params.has('subCategoryId') ||
            params.has('minPrice') ||
            params.has('maxPrice') ||
            (params.get('selectedBrand') && params.get('selectedBrand').trim().length > 0) ||
            params.has('inStockOnly');

        if (hasSearchOrFilter) {
            const resultsSection = document.querySelector('.search-results');
            if (resultsSection) {
                setTimeout(() => {
                    resultsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }, 0);
            }
        }
        
        // Animasyonları başlat
        initScrollAnimations();
        
        // Sepete ekle butonlarını dinle
        initAddToCartButtons();
        
        // Hızlı görüntüleme butonlarını dinle
        initQuickViewButtons();

        // Lazy loading için
        initLazyLoading();

    });

    /* ══════════════════════════════════════════════════════════════
       SCROLL ANİMASYONLARI
    ══════════════════════════════════════════════════════════════ */
    function initScrollAnimations() {
        const elements = document.querySelectorAll(
            '.product-card, .cat-card, .brand-pill, .trust-item'
        );

        if (!elements.length) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    setTimeout(() => {
                        entry.target.style.opacity = '0';
                        entry.target.style.transform = 'translateY(30px)';
                        entry.target.style.transition = 'all 0.6s ease';
                        
                        requestAnimationFrame(() => {
                            entry.target.style.opacity = '1';
                            entry.target.style.transform = 'translateY(0)';
                        });
                    }, index * 50);
                    
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        });

        elements.forEach(el => observer.observe(el));
    }

    /* ══════════════════════════════════════════════════════════════
       SEPETE EKLE FONKSİYONU
    ══════════════════════════════════════════════════════════════ */
    function initAddToCartButtons() {
        const buttons = document.querySelectorAll('.btn-add-cart');
        
        buttons.forEach(button => {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                
                const productId = this.dataset.productId;
                const productName = this.dataset.productName;
                
                // TODO: Gerçek sepet API'sine bağlan
                // Şimdilik kullanıcıya bildirim göster
                addToCart(productId, productName);
            });
        });
    }

    function addToCart(productId, productName) {
        // Animasyonlu buton feedback
        const button = document.querySelector(`[data-product-id="${productId}"]`);
        
        if (button) {
            const originalText = button.innerHTML;
            button.innerHTML = '<i class="bi bi-check-circle"></i> Eklendi!';
            button.style.background = '#2dc653';
            button.disabled = true;
            
            setTimeout(() => {
                button.innerHTML = originalText;
                button.style.background = '';
                button.disabled = false;
            }, 2000);
        }

        // Toast bildirimi göster
        showToast(`"${productName}" sepete eklendi! 🛒`, 'success');

        // TODO: AJAX ile backend'e gönder
        /*
        fetch('/Cart/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ productId: productId, quantity: 1 })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateCartBadge(data.cartItemCount);
            }
        });
        */
    }

    /* ══════════════════════════════════════════════════════════════
       HIZLI GÖRÜNTÜLEME (QUICK VIEW)
    ══════════════════════════════════════════════════════════════ */
    function initQuickViewButtons() {
        const buttons = document.querySelectorAll('.btn-quick-view');
        
        buttons.forEach(button => {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                
                const productId = this.dataset.productId;
                openQuickView(productId);
            });
        });
    }

    function openQuickView(productId) {
        // TODO: Modal açılacak ve AJAX ile ürün detayı gelecek
        console.log('Quick view açılıyor:', productId);
        showToast('Hızlı görüntüleme yakında eklenecek! 👀', 'info');
        
        // Örnek modal kodu:
        /*
        fetch(`/Products/QuickView/${productId}`)
            .then(response => response.text())
            .then(html => {
                // Modal içeriğini doldur
                document.getElementById('quickViewModal').innerHTML = html;
                // Modal'ı aç
                const modal = new bootstrap.Modal(document.getElementById('quickViewModal'));
                modal.show();
            });
        */
    }

    /* ══════════════════════════════════════════════════════════════
       TOAST BİLDİRİMLERİ
    ══════════════════════════════════════════════════════════════ */
    function showToast(message, type = 'success') {
        // Varsa eski toast'ı kaldır
        const existing = document.getElementById('customToast');
        if (existing) existing.remove();

        // Renk belirleme
        const colors = {
            success: '#2dc653',
            error: '#e63946',
            warning: '#ff6b35',
            info: '#1a1a2e'
        };

        const toast = document.createElement('div');
        toast.id = 'customToast';
        toast.style.cssText = `
            position: fixed;
            bottom: 24px;
            right: 24px;
            background: ${colors[type] || colors.info};
            color: #fff;
            padding: 1rem 1.5rem;
            border-radius: 12px;
            font-size: 0.95rem;
            font-weight: 600;
            box-shadow: 0 10px 40px rgba(0,0,0,0.3);
            z-index: 99999;
            animation: slideInUp 0.4s cubic-bezier(0.68, -0.55, 0.265, 1.55);
            max-width: 350px;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        `;
        
        toast.innerHTML = `
            <span style="font-size: 1.2rem;">${getToastIcon(type)}</span>
            <span>${message}</span>
        `;
        
        document.body.appendChild(toast);

        // 3 saniye sonra kapat
        setTimeout(() => {
            toast.style.animation = 'slideOutDown 0.4s ease-out';
            setTimeout(() => toast.remove(), 400);
        }, 3000);
    }

    function getToastIcon(type) {
        const icons = {
            success: '✓',
            error: '✕',
            warning: '⚠',
            info: 'ℹ'
        };
        return icons[type] || icons.info;
    }

    /* ══════════════════════════════════════════════════════════════
       LAZY LOADING
    ══════════════════════════════════════════════════════════════ */
    function initLazyLoading() {
        const images = document.querySelectorAll('img[loading="lazy"]');
        
        if ('loading' in HTMLImageElement.prototype) {
            // Tarayıcı native lazy loading destekliyorsa
            return;
        }

        // Fallback: IntersectionObserver ile
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.add('loaded');
                    imageObserver.unobserve(img);
                }
            });
        });

        images.forEach(img => imageObserver.observe(img));
    }

    /* ══════════════════════════════════════════════════════════════
       SEPET BADGE GÜNCELLEME
    ══════════════════════════════════════════════════════════════ */
    function updateCartBadge(count) {
        const badge = document.querySelector('.cart-badge');
        if (badge) {
            badge.textContent = count;
            badge.style.animation = 'pulse 0.5s ease';
            
            setTimeout(() => {
                badge.style.animation = '';
            }, 500);
        }
    }

    /* ══════════════════════════════════════════════════════════════
       CSS ANİMASYONLARI
    ══════════════════════════════════════════════════════════════ */
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideInUp {
            from {
                transform: translateY(100px);
                opacity: 0;
            }
            to {
                transform: translateY(0);
                opacity: 1;
            }
        }

        @keyframes slideOutDown {
            from {
                transform: translateY(0);
                opacity: 1;
            }
            to {
                transform: translateY(100px);
                opacity: 0;
            }
        }

        @keyframes pulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.2); }
        }
    `;
    document.head.appendChild(style);

})();
