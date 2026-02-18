using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ECommerce.DataAccess.Data;

namespace ECommerce.DataAccess.Factories
{
    /// <summary>
    /// Bu sınıf SADECE migration oluşturma zamanında kullanılır.
    /// Production'da DI Container üzerinden DbContext enjekte edilir.
    /// </summary>
    public class DesignTimeDbContextFactory 
        : IDesignTimeDbContextFactory<ECommerceDbContext>
    {
        public ECommerceDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ECommerceDbContext>();
            
            // SADECE LOCAL DEVELOPMENT İÇİN
            // Production'da bu connection string ASLA kullanılmaz
            optionsBuilder.UseSqlServer(
                @"Server=DESKTOP-PU4VJM0\SQLEXPRESS;Database=ECommerceData;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
            );

            return new ECommerceDbContext(optionsBuilder.Options);
        }
    }
}