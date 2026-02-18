using ECommerce.DataAccess;
using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.DataAccess.Identity;
using ECommerce.Models.Identity;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Session - Guest checkout için gerekli
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DbContext
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ECommerce.DataAccess")
    )
);

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policy - E-commerce friendly
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength         = 6;

    // Lockout
    options.Lockout.MaxFailedAccessAttempts = 5;
    //options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(10);
    options.Lockout.AllowedForNewUsers      = true;

    // User
    options.User.RequireUniqueEmail = true;

    // Sign-in
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ECommerceDbContext>()
.AddDefaultTokenProviders();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name     = "ECommerceAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.LoginPath        = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.SlidingExpiration = true;
    options.ExpireTimeSpan    = TimeSpan.FromMinutes(60);
});

// Repository ve Unit of Work services
builder.Services.AddRepositoryServices();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, GmailEmailSender>();
builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddControllersWithViews();

// AccountController için IUnitOfWork ekle
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Identity seed (otomatik Development'ta, optional Production'da)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var seeder = services.GetRequiredService<IdentitySeeder>();

    try
    {
        // Only auto-seed in Development environment
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Development environment detected, auto-seeding Identity data");
            await seeder.SeedAsync();
        }
        else
        {
            logger.LogInformation("Production environment detected, skipping auto-seed (use manual seeding endpoint if needed)");
        }
    }
    catch (Exception ex)
    {
        logger.LogError($"Critical error during Identity seeding: {ex.Message}. Application will continue with existing data.");
        // In production, we don't want to fail startup if seeding fails
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Session middleware
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
