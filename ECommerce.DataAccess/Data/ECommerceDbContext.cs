using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ECommerce.Models.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ECommerce.Models.Identity;

namespace ECommerce.DataAccess.Data
{
    public class ECommerceDbContext : IdentityDbContext<ApplicationUser>
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Slider> Sliders { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Mevcut model konfigürasyonlarında eksiklikler var, bu yüzden bu uyarıyı suppress ediyoruz
        optionsBuilder.ConfigureWarnings(w => 
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ===========
        // ==========================
        // CATEGORY SEED DATA
        // =====================================
        modelBuilder.Entity<Category>().HasData(
            // 🔹 ANA KATEGORİLER
            new Category 
            { 
                Id = 1, 
                Name = "Kadın Ayakkabı", 
                DisplayOrder = 1, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = null 
            },
            new Category 
            { 
                Id = 2, 
                Name = "Erkek Ayakkabı", 
                DisplayOrder = 2, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = null 
            },
            new Category 
            { 
                Id = 3, 
                Name = "Çocuk Ayakkabı", 
                DisplayOrder = 3, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = null 
            },

            // 🔹 KADIN ALT KATEGORİLERİ
            new Category 
            { 
                Id = 4, 
                Name = "Spor", 
                DisplayOrder = 1, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 1 
            },
            new Category 
            { 
                Id = 5, 
                Name = "Topuklu", 
                DisplayOrder = 2, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 1 
            },
            new Category 
            { 
                Id = 6, 
                Name = "Günlük", 
                DisplayOrder = 3, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 1 
            },

            // 🔹 ERKEK ALT KATEGORİLERİ
            new Category 
            { 
                Id = 7, 
                Name = "Spor", 
                DisplayOrder = 1, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 2 
            },
            new Category 
            { 
                Id = 8, 
                Name = "Klasik", 
                DisplayOrder = 2, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 2 
            },
            new Category 
            { 
                Id = 9, 
                Name = "Günlük", 
                DisplayOrder = 3, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 2 
            },

            // 🔹 ÇOCUK ALT KATEGORİLERİ
            new Category 
            { 
                Id = 10, 
                Name = "Spor", 
                DisplayOrder = 1, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 3 
            },
            new Category 
            { 
                Id = 11, 
                Name = "Okul", 
                DisplayOrder = 2, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 3 
            },
            new Category 
            { 
                Id = 12, 
                Name = "Günlük", 
                DisplayOrder = 3, 
                IsActive = true, 
                IsDeleted = false,
                ParentCategoryId = 3 
            }
        );

        // Product -> ProductVariant, ProductImage (WithMany() parametresiz: HasData'lı Product'a navigasyon eklenmez, convention ile collection çalışır)
        modelBuilder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany()
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductImage>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShoppingCart>()
            .HasOne(sc => sc.ProductVariant)
            .WithMany()
            .HasForeignKey(sc => sc.ProductVariantId)
            .OnDelete(DeleteBehavior.NoAction);

        // =====================================
        // PRODUCT SEED DATA
        // =====================================
        modelBuilder.Entity<Product>().HasData(
            // 👟 KADIN SPOR AYAKKABI
            new Product
            {
                Id = 1,
                Name = "Nike Air Max 270",
                Description = "Günlük kullanım için rahat ve şık spor ayakkabı",
                Brand = "Nike",
                CategoryId = 4,
                ListPrice = 3499.99M,
                DiscountRate = 15M,
                StockQuantity = 150,
                Color = "Beyaz",
                AvailableSizes = "36,37,38,39,40,41",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 15, 10, 0, 0)
            },
            new Product
            {
                Id = 2,
                Name = "Adidas Ultraboost",
                Description = "Koşu ve antrenman için ideal spor ayakkabı",
                Brand = "Adidas",
                CategoryId = 4,
                ListPrice = 4299.99M,
                DiscountRate = 10M,
                StockQuantity = 100,
                Color = "Siyah",
                AvailableSizes = "36,37,38,39,40",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 15, 11, 0, 0)
            },

            // 👠 KADIN TOPUKLU
            new Product
            {
                Id = 3,
                Name = "Hotiç Stiletto",
                Description = "Zarif ve şık topuklu ayakkabı",
                Brand = "Hotiç",
                CategoryId = 5,
                ListPrice = 1899.99M,
                DiscountRate = 20M,
                StockQuantity = 80,
                Color = "Kırmızı",
                AvailableSizes = "36,37,38,39,40",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 16, 9, 0, 0)
            },

            // 👞 ERKEK SPOR AYAKKABI
            new Product
            {
                Id = 4,
                Name = "Puma RS-X",
                Description = "Retro tasarımlı erkek spor ayakkabı",
                Brand = "Puma",
                CategoryId = 7,
                ListPrice = 2999.99M,
                DiscountRate = null,
                StockQuantity = 120,
                Color = "Lacivert",
                AvailableSizes = "40,41,42,43,44,45",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 16, 10, 0, 0)
            },
            new Product
            {
                Id = 5,
                Name = "New Balance 574",
                Description = "Klasik ve rahat erkek spor ayakkabı",
                Brand = "New Balance",
                CategoryId = 7,
                ListPrice = 3299.99M,
                DiscountRate = 12M,
                StockQuantity = 90,
                Color = "Gri",
                AvailableSizes = "40,41,42,43,44",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 17, 10, 0, 0)
            },

            // 👔 ERKEK KLASİK
            new Product
            {
                Id = 6,
                Name = "Altınyıldız Klasik Deri",
                Description = "İş ve özel günler için klasik deri ayakkabı",
                Brand = "Altınyıldız",
                CategoryId = 8,
                ListPrice = 2499.99M,
                DiscountRate = 25M,
                StockQuantity = 60,
                Color = "Kahverengi",
                AvailableSizes = "40,41,42,43,44",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 18, 14, 0, 0)
            },

            // 👧 ÇOCUK SPOR
            new Product
            {
                Id = 7,
                Name = "Kinetix Çocuk Spor",
                Description = "Dayanıklı ve rahat çocuk spor ayakkabısı",
                Brand = "Kinetix",
                CategoryId = 10,
                ListPrice = 899.99M,
                DiscountRate = 18M,
                StockQuantity = 200,
                Color = "Mavi",
                AvailableSizes = "28,29,30,31,32,33,34,35",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 20, 9, 0, 0)
            },

            // 🎒 ÇOCUK OKUL
            new Product
            {
                Id = 8,
                Name = "Polaris Okul Ayakkabısı",
                Description = "Okul için uygun siyah ayakkabı",
                Brand = "Polaris",
                CategoryId = 11,
                ListPrice = 699.99M,
                DiscountRate = null,
                StockQuantity = 150,
                Color = "Siyah",
                AvailableSizes = "28,29,30,31,32,33,34,35,36",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 22, 11, 0, 0)
            },

            // 🏃 KADIN GÜNLÜK
            new Product
            {
                Id = 9,
                Name = "Skechers Go Walk",
                Description = "Hafif ve rahat günlük yürüyüş ayakkabısı",
                Brand = "Skechers",
                CategoryId = 6,
                ListPrice = 1999.99M,
                DiscountRate = 22M,
                StockQuantity = 110,
                Color = "Pembe",
                AvailableSizes = "36,37,38,39,40",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 25, 13, 0, 0)
            },

            // 👟 ERKEK GÜNLÜK
            new Product
            {
                Id = 10,
                Name = "Converse Chuck Taylor",
                Description = "İkonik tasarımlı günlük spor ayakkabı",
                Brand = "Converse",
                CategoryId = 9,
                ListPrice = 1799.99M,
                DiscountRate = 8M,
                StockQuantity = 180,
                Color = "Beyaz",
                AvailableSizes = "39,40,41,42,43,44,45",
                ImageUrl = null,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2025, 1, 28, 16, 0, 0)
            },

            // --- 11-30: Ek ürünler ---
            new Product { Id = 11, Name = "Nike Revolution 7", Description = "Günlük koşu ve yürüyüş", Brand = "Nike", CategoryId = 4, ListPrice = 1299.99M, DiscountRate = 5M, StockQuantity = 95, Color = "Siyah", AvailableSizes = "36,37,38,39,40", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 1, 9, 0, 0) },
            new Product { Id = 12, Name = "Adidas Runfalcon", Description = "Hafif koşu ayakkabısı", Brand = "Adidas", CategoryId = 4, ListPrice = 1499.99M, DiscountRate = null, StockQuantity = 70, Color = "Beyaz", AvailableSizes = "36,37,38,39,40,41", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 1, 10, 0, 0) },
            new Product { Id = 13, Name = "Hotiç Topuklu Pabuç", Description = "Ofis ve özel günler için", Brand = "Hotiç", CategoryId = 5, ListPrice = 1599.99M, DiscountRate = 15M, StockQuantity = 55, Color = "Siyah", AvailableSizes = "36,37,38,39", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 2, 11, 0, 0) },
            new Product { Id = 14, Name = "Skechers D'lites", Description = "Platform tabanlı rahat ayakkabı", Brand = "Skechers", CategoryId = 6, ListPrice = 2199.99M, DiscountRate = 10M, StockQuantity = 88, Color = "Beyaz", AvailableSizes = "36,37,38,39,40", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 2, 14, 0, 0) },
            new Product { Id = 15, Name = "Puma Smash", Description = "Klasik spor stil", Brand = "Puma", CategoryId = 7, ListPrice = 999.99M, DiscountRate = 20M, StockQuantity = 130, Color = "Beyaz", AvailableSizes = "40,41,42,43,44", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 3, 9, 0, 0) },
            new Product { Id = 16, Name = "New Balance 530", Description = "Retro koşu ayakkabısı", Brand = "New Balance", CategoryId = 7, ListPrice = 2799.99M, DiscountRate = 8M, StockQuantity = 65, Color = "Gri", AvailableSizes = "40,41,42,43,44,45", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 3, 10, 0, 0) },
            new Product { Id = 17, Name = "Altınyıldız Babet", Description = "Sade deri babet", Brand = "Altınyıldız", CategoryId = 8, ListPrice = 899.99M, DiscountRate = null, StockQuantity = 90, Color = "Siyah", AvailableSizes = "40,41,42,43,44", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 4, 11, 0, 0) },
            new Product { Id = 18, Name = "Kinetix Çocuk Basketbol", Description = "Saha ve salon için", Brand = "Kinetix", CategoryId = 10, ListPrice = 749.99M, DiscountRate = 12M, StockQuantity = 120, Color = "Kırmızı", AvailableSizes = "28,29,30,31,32,33,34", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 4, 14, 0, 0) },
            new Product { Id = 19, Name = "Polaris Çocuk Bot", Description = "Kışlık su geçirmez bot", Brand = "Polaris", CategoryId = 11, ListPrice = 599.99M, DiscountRate = 25M, StockQuantity = 85, Color = "Lacivert", AvailableSizes = "28,29,30,31,32,33", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 5, 9, 0, 0) },
            new Product { Id = 20, Name = "Nike Air Force 1", Description = "İkonik basketbol tarzı", Brand = "Nike", CategoryId = 9, ListPrice = 3299.99M, DiscountRate = 5M, StockQuantity = 75, Color = "Beyaz", AvailableSizes = "39,40,41,42,43,44,45", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 5, 12, 0, 0) },
            new Product { Id = 21, Name = "Adidas Gazelle", Description = "Klasik spor ayakkabı", Brand = "Adidas", CategoryId = 7, ListPrice = 2499.99M, DiscountRate = 10M, StockQuantity = 60, Color = "Mavi", AvailableSizes = "40,41,42,43,44", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 6, 10, 0, 0) },
            new Product { Id = 22, Name = "Hotiç Abiye Topuklu", Description = "Gece ve davet için", Brand = "Hotiç", CategoryId = 5, ListPrice = 2299.99M, DiscountRate = 18M, StockQuantity = 40, Color = "Altın", AvailableSizes = "36,37,38,39,40", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 6, 15, 0, 0) },
            new Product { Id = 23, Name = "Skechers Arch Fit", Description = "Destekli taban teknolojisi", Brand = "Skechers", CategoryId = 6, ListPrice = 1899.99M, DiscountRate = null, StockQuantity = 72, Color = "Gri", AvailableSizes = "36,37,38,39,40,41", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 7, 9, 0, 0) },
            new Product { Id = 24, Name = "Puma Suede Classic", Description = "Retro sneaker", Brand = "Puma", CategoryId = 9, ListPrice = 1599.99M, DiscountRate = 15M, StockQuantity = 98, Color = "Kahverengi", AvailableSizes = "39,40,41,42,43,44", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 7, 11, 0, 0) },
            new Product { Id = 25, Name = "New Balance 327", Description = "Unisex günlük spor", Brand = "New Balance", CategoryId = 4, ListPrice = 2399.99M, DiscountRate = 7M, StockQuantity = 82, Color = "Bej", AvailableSizes = "36,37,38,39,40,41", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 8, 10, 0, 0) },
            new Product { Id = 26, Name = "Altınyıldız Loafer", Description = "İş ve günlük loafer", Brand = "Altınyıldız", CategoryId = 8, ListPrice = 1199.99M, DiscountRate = 20M, StockQuantity = 68, Color = "Bordö", AvailableSizes = "40,41,42,43,44", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 8, 14, 0, 0) },
            new Product { Id = 27, Name = "Kinetix Çocuk Günlük", Description = "Okul sonrası rahat ayakkabı", Brand = "Kinetix", CategoryId = 12, ListPrice = 549.99M, DiscountRate = 10M, StockQuantity = 150, Color = "Yeşil", AvailableSizes = "28,29,30,31,32,33,34,35", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 9, 9, 0, 0) },
            new Product { Id = 28, Name = "Polaris Çocuk Spor", Description = "Hafif ve nefes alan", Brand = "Polaris", CategoryId = 10, ListPrice = 449.99M, DiscountRate = null, StockQuantity = 110, Color = "Turuncu", AvailableSizes = "28,29,30,31,32,33,34", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 9, 12, 0, 0) },
            new Product { Id = 29, Name = "Nike Blazer Mid", Description = "Vintage basketbol tarzı", Brand = "Nike", CategoryId = 7, ListPrice = 2699.99M, DiscountRate = 12M, StockQuantity = 58, Color = "Beyaz", AvailableSizes = "40,41,42,43,44,45", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 10, 10, 0, 0) },
            new Product { Id = 30, Name = "Converse Run Star", Description = "Platform tabanlı Converse", Brand = "Converse", CategoryId = 9, ListPrice = 2299.99M, DiscountRate = 5M, StockQuantity = 78, Color = "Siyah", AvailableSizes = "36,37,38,39,40,41,42,43,44", ImageUrl = null, IsActive = true, IsDeleted = false, CreatedDate = new DateTime(2025, 2, 10, 15, 0, 0) }
        );

        modelBuilder.Entity<Slider>().HasData(
            new Slider
            {
                Id = 1,
                Title = "Yaz İndirimi Başladı!",
                Description = "Tüm ürünlerde %50'ye varan indirim fırsatını kaçırmayın.",
                ImageUrl = @"\images\slider\100.jpg",
                DisplayOrder = 1,
                IsActive = true,
                
            },
            new Slider
            {
                Id = 2,
                Title = "Yeni Sezon Ürünleri Geldi!",
                Description = "2025 Sonbahar/Kış koleksiyonumuzla tarzınızı yenileyin.",
                ImageUrl = @"\images\slider\199.jpg",
                DisplayOrder = 2,
                IsActive = true,
               
            },
            new Slider
            {
                Id = 3,
                Title = "Sadece Bugün: Ekstra %10 İndirim!",
                Description = "Sepette ekstra %10 indirim fırsatını yakalayın.",
                ImageUrl = @"\images\slider\336.jpg",
                DisplayOrder = 3,
                IsActive = true,
               
            }
        );

        


    }
    }
}
