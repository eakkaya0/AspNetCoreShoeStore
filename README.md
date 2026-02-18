# E-Commerce Platform

![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
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

Bu proje, ASP.NET Core 9.0 kullanarak geliştirilmiş modern bir ayakkabı e-ticaret platformudur. Temel amacı, kullanıcıların ayakkabı ürünlerini görüntüleyebilmesi, sepete ekleyebilmesi, sipariş verebilmesi ve yöneticilerin sistem yönetimini yapabildiği kapsamlı bir e-ticaret çözümü sunmaktır.

### Ana Hedefler
- **Performans**: Optimiz edilmiş sorgular ve caching stratejileri
- **Güvenlik**: Role-based authentication ve authorization
- **Ölçeklenebilirlik**: Katmanlı mimari ve dependency injection
- **Kullanıcı Deneyimi**: Modern ve responsive arayüz
- **Yönetilebilirlik**: Admin paneli ve raporlama özellikleri

## 🛠 Teknolojiler

### Backend
- **.NET 9.0** - Framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core 9.0** - ORM
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
├── ECommerce.Services/           # Business logic katmanı
└── ECommerce.Utility/            # Utility katmanı
    ├── Extensions/               # Extension metodlar
    ├── Helpers/                  # Helper sınıflar
    └── Constants/                # Sabit değerler
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

#### 4. Utility Layer
- **Extensions**: Extension metodlar (örn: string, datetime)
- **Helpers**: Yardımcı sınıflar (örn: email, file operations)
- **Constants**: Sabit değerler ve enum'lar

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
- **Ayakkabı Ürün Yönetimi**: CRUD işlemleri, beden varyantları, soft delete
- **Kategori Yönetimi**: Ana ve alt kategori yapısı (spor, günlük, klasik vb.)
- **Sepet Sistemi**: Guest ve kullanıcı sepet yönetimi
- **Sipariş Yönetimi**: Sipariş takibi, durum yönetimi
- **Kullanıcı Yönetimi**: Registration, authentication, rol yönetimi

### 🛒 E-Ticaret Özellikleri
- **Ayakkabı Varyantları**: Beden (36-45) ve renk seçenekleri
- **Stok Yönetimi**: Beden bazında stok takibi ve otomatik stok düşme
- **Fiyatlandırma**: İndirimli fiyatlar ve vergi hesaplaması
- **Arama ve Filtreleme**: Marka, beden, renk filtreleme
- **Ödeme Sistemi**: (Gelecekte entegrasyon)

### 👤 Kullanıcı Özellikleri
- **Guest Checkout**: Kayıtsız kullanıcı alışverişi
- **User Profiles**: Kullanıcı profilleri ve sipariş geçmişi
- **Role-Based Access**: Admin ve müşteri rolleri
- **Email Verification**: Email doğrulama sistemi
- **Slider Yönetimi**: Ana sayfa slider'larını ekleme/düzenme/silme

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
- .NET 9.0 SDK
- Visual Studio 2022 veya VS Code
- SQL Server 2022 veya SQL Server Express

### Adım 1: Repository'yi Klonlayın
```bash
git clone https://github.com/eakkaya0/ECommerce.git
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
<img width="1918" height="916" alt="anasayfa" src="https://github.com/user-attachments/assets/5cc55b6e-4830-4112-91fc-38951324248a" />



### Ürün Listesi
<!-- Ürün listesi ekran görüntüsü buraya eklenecek -->
<img width="1902" height="827" alt="yeniürünler" src="https://github.com/user-attachments/assets/28c08674-a907-45f9-acfa-3679a1a45145" />

<img width="1912" height="958" alt="inidirmliürünler" src="https://github.com/user-attachments/assets/98871584-3fd0-4a70-acb5-c7d4fd7a5ce2" />

<img width="1918" height="972" alt="coksatanlar" src="https://github.com/user-attachments/assets/705fbd97-5ee4-47df-b9fa-f7c55cb26882" />

<img width="1918" height="967" alt="tumurunler" src="https://github.com/user-attachments/assets/cfdedda5-c50f-45ec-9f48-fb5f6667b1d9" />








### Ürün Detayı
<!-- Ürün detayı ekran görüntüsü buraya eklenecek -->
<img width="1917" height="966" alt="urundetay" src="https://github.com/user-attachments/assets/569eda81-e7a1-4145-9d53-a3e0464a933b" />



### Sepet
<!-- Sepet ekran görüntüsü buraya eklenecek -->
<img width="1918" height="952" alt="sepet" src="https://github.com/user-attachments/assets/c43db180-f24e-42d4-8c90-15814efe2ecd" />



### Checkout
<!-- Checkout ekran görüntüsü buraya eklenecek -->
<img width="1918" height="952" alt="checkout" src="https://github.com/user-attachments/assets/2cd64dc1-920a-43d0-ab08-2ad67407c4eb" />



### Ürün Yönetimi
<!-- Ürün yönetimi ekran görüntüsü buraya eklenecek -->
<img width="1865" height="957" alt="adminürünlistesi" src="https://github.com/user-attachments/assets/b62aef77-14af-4c7c-8b85-46cc1a8ae6ab" />

<img width="1918" height="953" alt="üründüzenleme1" src="https://github.com/user-attachments/assets/c481b84d-90d5-4d36-82d2-c8d013ad9d8c" />

<img width="1911" height="963" alt="üründüzenleme2" src="https://github.com/user-attachments/assets/ee8ede11-634a-4339-a8c8-5b9a43ebdd60" />

<img width="1917" height="953" alt="üründüzenleme3" src="https://github.com/user-attachments/assets/390d576f-9ee4-4528-a3cc-5abbaa15d0f2" />

<img width="1918" height="962" alt="kategori" src="https://github.com/user-attachments/assets/9a64e15b-75c7-4d4b-bc9c-108688a137ee" />






### Sipariş Yönetimi
<!-- Sipariş yönetimi ekran görüntüsü buraya eklenecek -->
<img width="1917" height="988" alt="siparisyonetimi" src="https://github.com/user-attachments/assets/1fb1bbde-2d8e-4e8c-8299-bd617161fa6e" />

<img width="1912" height="923" alt="siparisyonetimidetay" src="https://github.com/user-attachments/assets/5d264281-cf51-4b35-98ca-2f38b634905e" />






### Kullanıcı Yönetimi
<!-- Kullanıcı yönetimi ekran görüntüsü buraya eklenecek -->
<img width="1918" height="945" alt="kullanıcıyönetimiana" src="https://github.com/user-attachments/assets/a5b3fce6-1c63-4246-8e3e-2a2941bc0e02" />

<img width="1912" height="953" alt="kullanıcıyönetimidetay" src="https://github.com/user-attachments/assets/3cce382d-17ce-46de-a8a5-f3f1c620824c" />



### Slider Yönetimi
<!-- Slider yönetimi ekran görüntüsü buraya eklenecek -->
<img width="1918" height="957" alt="slieryönetimi" src="https://github.com/user-attachments/assets/41d5ac8d-b2f9-437f-9b96-f15a10262b91" />


## 🗄 Veritabanı Şeması

### Ana Tablolar
- **Users**: Kullanıcı bilgileri (ASP.NET Core Identity)
- **Products**: Ayakkabı ürün bilgileri
- **Categories**: Kategori bilgileri (spor, günlük, klasik vb.)
- **ProductVariants**: Ayakkabı beden ve renk varyantları
- **ShoppingCarts**: Sepetler
- **Orders**: Siparişler
- **OrderItems**: Sipariş kalemleri
- **Sliders**: Ana sayfa slider'ları

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
- **SliderController**: Ana sayfa slider yönetimi
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
GET /Slider/Index                   - Slider yönetimi
GET /Slider/Upsert/{id?}           - Slider ekleme/düzenleme
POST /Slider/Upsert                 - Slider kaydet
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

- **Emre Akkaya** - *Initial work* - [GitHub Profile](https://github.com/eakkaya0)

## 🙏 Teşekkür

- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Web framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM
- [Bootstrap](https://getbootstrap.com/) - CSS framework
- [Font Awesome](https://fontawesome.com/) - Icon library

---

⭐ Eğer bu proje işinize yaradıysa lütfen bir star verin!
