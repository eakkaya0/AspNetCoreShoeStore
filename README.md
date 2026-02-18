# E-Commerce Platform

![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-blue.svg)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-green.svg)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red.svg)

Modern, katmanlı mimariye sahip, SOLID prensiplerine uygun geliştirilmiş tam teşekküllü ayakkabı e-ticaret platformu.

## 📋 İçerik

- [Proje Hakkında](#-proje-hakkında)
- [Teknolojiler](#-teknolojiler)
- [Katmanlı Mimari](#-katmanlı-mimari)
- [SOLID Prensipleri](#-solid-prensipleri)
- [Unit of Work Pattern](#-unit-of-work-pattern)
- [Özellikler](#-özellikler)
- [Kurulum](#-kurulum)
- [Ekran Görüntüleri](#-ekran-görüntüleri)
- [Veritabanı Şeması](#-veritabanı-şeması)
- [API Dokümantasyonu](#-api-dokümantasyonu)

## 🎯 Proje Hakkında

Bu proje, ASP.NET Core 8.0 kullanarak geliştirilmiş modern bir e-ticaret platformudur. Temel amacı, kullanıcıların ürünleri görüntüleyebilmesi, sepete ekleyebilmesi, sipariş verebilmesi ve yöneticilerin sistem yönetimini yapabildiği kapsamlı bir e-ticaret çözümü sunmaktır.

### Ana Hedefler
- **Performans**: Optimiz edilmiş sorgular ve caching stratejileri
- **Güvenlik**: Role-based authentication ve authorization
- **Ölçeklenebilirlik**: Katmanlı mimari ve dependency injection
- **Kullanıcı Deneyimi**: Modern ve responsive arayüz
- **Yönetilebilirlik**: Admin paneli ve raporlama özellikleri

## 🛠 Teknolojiler

### Backend
- **.NET 8.0** - Framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Veritabanı
- **ASP.NET Core Identity** - Authentication & Authorization
- **AutoMapper** - Object mapping
- **FluentValidation** - Validation

### Frontend
- **Bootstrap 5** - CSS framework
- **jQuery** - JavaScript library
- **Font Awesome** - Icons
- **DataTables** - Tablo yönetimi
- **Toast notifications** - Bildirimler

### Development Tools
- **Visual Studio 2022** - IDE
- **Git** - Version control
- **GitHub** - Repository

## 🏗 Katmanlı Mimari

Proje, katmanlı mimari prensiplerine göre tasarlanmıştır:

```
ECommerce/
├── ECommerce.Models/              # Entity ve ViewModels
├── ECommerce.DataAccess/          # Veri erişim katmanı
│   ├── Repository/
│   │   ├── IRepository/          # Repository interface'leri
│   │   └── Repository/           # Repository implementasyonları
│   └── Data/
│       └── ECommerceDbContext.cs # DbContext
├── ECommerceWeb/                  # Presentation katmanı
│   ├── Controllers/              # MVC Controllers
│   ├── Views/                    # Razor Views
│   ├── wwwroot/                  # Static assets
│   └── Program.cs               # Application configuration
└── ECommerce.Services/           # Business logic katmanı (gelecekte)
```

### Katmanların Sorumlulukları

#### 1. Models Layer
- **Entity'ler**: Veritabanı tablolarını temsil eden sınıflar
- **ViewModel'ler**: View'lar için özel modeller
- **DTO'lar**: Veri transfer objeleri

#### 2. DataAccess Layer
- **Repository Pattern**: Veri erişim soyutlaması
- **Unit of Work**: Transaction yönetimi
- **DbContext**: Entity Framework konfigürasyonu

#### 3. Presentation Layer
- **Controllers**: HTTP isteklerini işler
- **Views**: Kullanıcı arayüzü
- **Static Assets**: CSS, JS, resimler

## 📐 SOLID Prensipleri

### 1. Single Responsibility Principle (SRP)
Her sınıfın tek bir sorumluluğu vardır:
- `ProductRepository`: Sadece ürün verilerini yönetir
- `OrderController`: Sadece sipariş işlemlerini yönetir
- `UserListViewModel`: Sadece kullanıcı listesi verisini tutar

### 2. Open/Closed Principle (OCP)
Sınıflar gelişime açık, değişime kapalıdır:
- `IRepository<T>` interface'i yeni repository'ler eklenmesine izin verir
- `UnitOfWork` pattern yeni repository'ler eklenmeden genişletilebilir

### 3. Liskov Substitution Principle (LSP)
Türetilmiş sınıflar, temel sınıfların yerini alabilir:
- `Repository<T>` sınıfı `IRepository<T>` interface'inin tüm özelliklerini implemente eder
- Herhangi bir `IRepository<T>` implementasyonu birbiriyle değiştirilebilir

### 4. Interface Segregation Principle (ISP)
Interface'ler spesifik olmalıdır:
```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<Product?> GetWithDetailsAsync(int id);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
    Task<Order?> GetOrderWithDetailsAsync(int id);
}
```

### 5. Dependency Inversion Principle (DIP)
Yüksek seviyeli modüller, düşük seviyeli modüllere bağlı olmamalıdır:
- Controller'lar doğrudan repository'lere değil, interface'lere bağlıdır
- Dependency Injection ile bağımlılıklar yönetilir

## 🔄 Unit of Work Pattern

### Amaç
Birden fazla repository işlemini tek bir transaction içinde yönetmek.

### Implementasyon
```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Product { get; }
    ICategoryRepository Category { get; }
    IOrderRepository Order { get; }
    IShoppingCartRepository ShoppingCart { get; }
    
    Task<int> SaveAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    
    public IProductRepository Product { get; private set; }
    public ICategoryRepository Category { get; private set; }
    // ... diğer repository'ler
    
    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

### Kullanım
```csharp
// Controller'da kullanım
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<IActionResult> CompleteOrder(OrderDetailsVM model)
    {
        // Birden fazla repository işlemi
        var order = new Order { ... };
        await _unitOfWork.Order.AddAsync(order);
        
        foreach (var item in cartItems)
        {
            var orderItem = new OrderItem { ... };
            await _unitOfWork.OrderItem.AddAsync(orderItem);
        }
        
        // Tek bir transaction ile kaydet
        await _unitOfWork.SaveAsync();
    }
}
```

## ✨ Özellikler

### 🏪 Ana Özellikler
- **Ürün Yönetimi**: CRUD işlemleri, varyant desteği, soft delete
- **Kategori Yönetimi**: Ana ve alt kategori yapısı
- **Sepet Sistemi**: Guest ve kullanıcı sepet yönetimi
- **Sipariş Yönetimi**: Sipariş takibi, durum yönetimi
- **Kullanıcı Yönetimi**: Registration, authentication, rol yönetimi

### 🛒 E-Ticaret Özellikleri
- **Ürün Varyantları**: Beden/renk gibi varyant desteği
- **Stok Yönetimi**: Otomatik stok düşme ve kontrol
- **Fiyatlandırma**: İndirimli fiyatlar ve vergi hesaplaması
- **Arama ve Filtreleme**: Gelişmiş arama özellikleri
- **Ödeme Sistemi**: (Gelecekte entegrasyon)

### 👤 Kullanıcı Özellikleri
- **Guest Checkout**: Kayıtsız kullanıcı alışverişi
- **User Profiles**: Kullanıcı profilleri ve sipariş geçmişi
- **Role-Based Access**: Admin ve müşteri rolleri
- **Email Verification**: Email doğrulama sistemi

### 📊 Admin Özellikleri
- **Dashboard**: İstatistikler ve grafikler
- **Ürün Yönetimi**: Ürün ekleme/düzenme/silme
- **Sipariş Yönetimi**: Sipariş görüntüleme/durum güncelleme
- **Kullanıcı Yönetimi**: Kullanıcı listesi ve rol atama
- **Raporlama**: Satış raporları ve analizler

### 🔧 Teknik Özellikler
- **Soft Delete**: Veri kaybını önleyen pasifleştirme sistemi
- **Session Management**: Guest sepet yönetimi için session
- **Image Upload**: Ürün görselleri için resim yükleme
- **Validation**: FluentValidation ile veri doğrulama
- **Error Handling**: Global error handling ve logging

## 🚀 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- Visual Studio 2022 veya VS Code
- SQL Server 2019+ veya SQL Server Express

### Adım 1: Repository'yi Klonlayın
```bash
git clone https://github.com/kullanici-adiniz/ECommerce.git
cd ECommerce
```

### Adım 2: Veritabanını Yapılandırın
`appsettings.json` dosyasında connection string'i güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ECommerceDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Adım 3: Migration'ları Uygulayın
```bash
dotnet ef database update
```

### Adım 4: Uygulamayı Çalıştırın
```bash
dotnet run
```

### Adım 5: Tarayıcıda Açın
```
https://localhost:7123
```

## 📸 Ekran Görüntüleri

### Ana Sayfa
<!-- Ana sayfa ekran görüntüsü buraya eklenecek -->
![Ana Sayfa](screenshots/homepage.png)

### Ürün Listesi
<!-- Ürün listesi ekran görüntüsü buraya eklenecek -->
![Ürün Listesi](screenshots/product-list.png)

### Ürün Detayı
<!-- Ürün detayı ekran görüntüsü buraya eklenecek -->
![Ürün Detayı](screenshots/product-detail.png)

### Sepet
<!-- Sepet ekran görüntüsü buraya eklenecek -->
![Sepet](screenshots/shopping-cart.png)

### Checkout
<!-- Checkout ekran görüntüsü buraya eklenecek -->
![Checkout](screenshots/checkout.png)

### Admin Dashboard
<!-- Admin dashboard ekran görüntüsü buraya eklenecek -->
![Admin Dashboard](screenshots/admin-dashboard.png)

### Ürün Yönetimi
<!-- Ürün yönetimi ekran görüntüsü buraya eklenecek -->
![Ürün Yönetimi](screenshots/product-management.png)

### Sipariş Yönetimi
<!-- Sipariş yönetimi ekran görüntüsü buraya eklenecek -->
![Sipariş Yönetimi](screenshots/order-management.png)

### Kullanıcı Yönetimi
<!-- Kullanıcı yönetimi ekran görüntüsü buraya eklenecek -->
![Kullanıcı Yönetimi](screenshots/user-management.png)

## 🗄 Veritabanı Şeması

### Ana Tablolar
- **Users**: Kullanıcı bilgileri (ASP.NET Core Identity)
- **Products**: Ürün bilgileri
- **Categories**: Kategori bilgileri
- **ProductVariants**: Ürün varyantları (beden, renk vb.)
- **ShoppingCarts**: Sepetler
- **Orders**: Siparişler
- **OrderItems**: Sipariş kalemleri

### İlişkiler
```
Users 1:N Orders
Users 1:N ShoppingCarts
Products 1:N ProductVariants
Products 1:N OrderItems
Products N:M Categories
Orders 1:N OrderItems
```

## 📚 API Dokümantasyonu

### Controller'lar
- **HomeController**: Ana sayfa ve ürün listeleme
- **ProductController**: Ürün yönetimi (CRUD)
- **CategoryController**: Kategori yönetimi
- **ShoppingCartController**: Sepet işlemleri
- **OrderController**: Sipariş yönetimi
- **AccountController**: Kullanıcı işlemleri
- **AdminController**: Admin paneli

### Önemli Endpoint'ler
```
GET /Home/Index                    - Ana sayfa
GET /Product/Details/{id}           - Ürün detayı
POST /ShoppingCart/AddToCart        - Sepete ekle
GET /Order/Checkout                - Checkout sayfası
POST /Order/CompleteOrder           - Sipariş tamamla
GET /Admin/Index                    - Admin paneli
```

## 🤝 Katkıda Bulunma

1. Repository'yi fork edin
2. Yeni bir branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some AmazingFeature'`)
4. Branch'e push edin (`git push origin feature/AmazingFeature`)
5. Bir Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında dağıtılmaktadır. Daha fazla bilgi için [LICENSE](LICENSE) dosyasını inceleyin.

## 👨‍💻 Geliştirici

- **Ad Soyad** - *Initial work* - [GitHub Profile](https://github.com/kullanici-adiniz)

## 🙏 Teşekkür

- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Web framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM
- [Bootstrap](https://getbootstrap.com/) - CSS framework
- [Font Awesome](https://fontawesome.com/) - Icon library

---

⭐ Eğer bu proje işinize yaradıysa lütfen bir star verin!