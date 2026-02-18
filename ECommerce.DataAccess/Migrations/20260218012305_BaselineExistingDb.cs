using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ECommerce.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BaselineExistingDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sliders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sliders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestFirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ListPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AvailableSizes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ProductId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId1",
                        column: x => x.ProductId1,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    PriceOverride = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProductId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId1",
                        column: x => x.ProductId1,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GuestSessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "IsDeleted", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { 1, 1, true, false, "Kadın Ayakkabı", null },
                    { 2, 2, true, false, "Erkek Ayakkabı", null },
                    { 3, 3, true, false, "Çocuk Ayakkabı", null }
                });

            migrationBuilder.InsertData(
                table: "Sliders",
                columns: new[] { "Id", "Description", "DisplayOrder", "ImageUrl", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Tüm ürünlerde %50'ye varan indirim fırsatını kaçırmayın.", 1, "\\images\\slider\\100.jpg", true, "Yaz İndirimi Başladı!" },
                    { 2, "2025 Sonbahar/Kış koleksiyonumuzla tarzınızı yenileyin.", 2, "\\images\\slider\\199.jpg", true, "Yeni Sezon Ürünleri Geldi!" },
                    { 3, "Sepette ekstra %10 indirim fırsatını yakalayın.", 3, "\\images\\slider\\336.jpg", true, "Sadece Bugün: Ekstra %10 İndirim!" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "IsDeleted", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { 4, 1, true, false, "Spor", 1 },
                    { 5, 2, true, false, "Topuklu", 1 },
                    { 6, 3, true, false, "Günlük", 1 },
                    { 7, 1, true, false, "Spor", 2 },
                    { 8, 2, true, false, "Klasik", 2 },
                    { 9, 3, true, false, "Günlük", 2 },
                    { 10, 1, true, false, "Spor", 3 },
                    { 11, 2, true, false, "Okul", 3 },
                    { 12, 3, true, false, "Günlük", 3 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AvailableSizes", "Brand", "CategoryId", "Color", "CreatedDate", "Description", "DiscountRate", "ImageUrl", "IsActive", "IsDeleted", "ListPrice", "Name", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "36,37,38,39,40,41", "Nike", 4, "Beyaz", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), "Günlük kullanım için rahat ve şık spor ayakkabı", 15m, null, true, false, 3499.99m, "Nike Air Max 270", 150 },
                    { 2, "36,37,38,39,40", "Adidas", 4, "Siyah", new DateTime(2025, 1, 15, 11, 0, 0, 0, DateTimeKind.Unspecified), "Koşu ve antrenman için ideal spor ayakkabı", 10m, null, true, false, 4299.99m, "Adidas Ultraboost", 100 },
                    { 3, "36,37,38,39,40", "Hotiç", 5, "Kırmızı", new DateTime(2025, 1, 16, 9, 0, 0, 0, DateTimeKind.Unspecified), "Zarif ve şık topuklu ayakkabı", 20m, null, true, false, 1899.99m, "Hotiç Stiletto", 80 },
                    { 4, "40,41,42,43,44,45", "Puma", 7, "Lacivert", new DateTime(2025, 1, 16, 10, 0, 0, 0, DateTimeKind.Unspecified), "Retro tasarımlı erkek spor ayakkabı", null, null, true, false, 2999.99m, "Puma RS-X", 120 },
                    { 5, "40,41,42,43,44", "New Balance", 7, "Gri", new DateTime(2025, 1, 17, 10, 0, 0, 0, DateTimeKind.Unspecified), "Klasik ve rahat erkek spor ayakkabı", 12m, null, true, false, 3299.99m, "New Balance 574", 90 },
                    { 6, "40,41,42,43,44", "Altınyıldız", 8, "Kahverengi", new DateTime(2025, 1, 18, 14, 0, 0, 0, DateTimeKind.Unspecified), "İş ve özel günler için klasik deri ayakkabı", 25m, null, true, false, 2499.99m, "Altınyıldız Klasik Deri", 60 },
                    { 7, "28,29,30,31,32,33,34,35", "Kinetix", 10, "Mavi", new DateTime(2025, 1, 20, 9, 0, 0, 0, DateTimeKind.Unspecified), "Dayanıklı ve rahat çocuk spor ayakkabısı", 18m, null, true, false, 899.99m, "Kinetix Çocuk Spor", 200 },
                    { 8, "28,29,30,31,32,33,34,35,36", "Polaris", 11, "Siyah", new DateTime(2025, 1, 22, 11, 0, 0, 0, DateTimeKind.Unspecified), "Okul için uygun siyah ayakkabı", null, null, true, false, 699.99m, "Polaris Okul Ayakkabısı", 150 },
                    { 9, "36,37,38,39,40", "Skechers", 6, "Pembe", new DateTime(2025, 1, 25, 13, 0, 0, 0, DateTimeKind.Unspecified), "Hafif ve rahat günlük yürüyüş ayakkabısı", 22m, null, true, false, 1999.99m, "Skechers Go Walk", 110 },
                    { 10, "39,40,41,42,43,44,45", "Converse", 9, "Beyaz", new DateTime(2025, 1, 28, 16, 0, 0, 0, DateTimeKind.Unspecified), "İkonik tasarımlı günlük spor ayakkabı", 8m, null, true, false, 1799.99m, "Converse Chuck Taylor", 180 },
                    { 11, "36,37,38,39,40", "Nike", 4, "Siyah", new DateTime(2025, 2, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), "Günlük koşu ve yürüyüş", 5m, null, true, false, 1299.99m, "Nike Revolution 7", 95 },
                    { 12, "36,37,38,39,40,41", "Adidas", 4, "Beyaz", new DateTime(2025, 2, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "Hafif koşu ayakkabısı", null, null, true, false, 1499.99m, "Adidas Runfalcon", 70 },
                    { 13, "36,37,38,39", "Hotiç", 5, "Siyah", new DateTime(2025, 2, 2, 11, 0, 0, 0, DateTimeKind.Unspecified), "Ofis ve özel günler için", 15m, null, true, false, 1599.99m, "Hotiç Topuklu Pabuç", 55 },
                    { 14, "36,37,38,39,40", "Skechers", 6, "Beyaz", new DateTime(2025, 2, 2, 14, 0, 0, 0, DateTimeKind.Unspecified), "Platform tabanlı rahat ayakkabı", 10m, null, true, false, 2199.99m, "Skechers D'lites", 88 },
                    { 15, "40,41,42,43,44", "Puma", 7, "Beyaz", new DateTime(2025, 2, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), "Klasik spor stil", 20m, null, true, false, 999.99m, "Puma Smash", 130 },
                    { 16, "40,41,42,43,44,45", "New Balance", 7, "Gri", new DateTime(2025, 2, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), "Retro koşu ayakkabısı", 8m, null, true, false, 2799.99m, "New Balance 530", 65 },
                    { 17, "40,41,42,43,44", "Altınyıldız", 8, "Siyah", new DateTime(2025, 2, 4, 11, 0, 0, 0, DateTimeKind.Unspecified), "Sade deri babet", null, null, true, false, 899.99m, "Altınyıldız Babet", 90 },
                    { 18, "28,29,30,31,32,33,34", "Kinetix", 10, "Kırmızı", new DateTime(2025, 2, 4, 14, 0, 0, 0, DateTimeKind.Unspecified), "Saha ve salon için", 12m, null, true, false, 749.99m, "Kinetix Çocuk Basketbol", 120 },
                    { 19, "28,29,30,31,32,33", "Polaris", 11, "Lacivert", new DateTime(2025, 2, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), "Kışlık su geçirmez bot", 25m, null, true, false, 599.99m, "Polaris Çocuk Bot", 85 },
                    { 20, "39,40,41,42,43,44,45", "Nike", 9, "Beyaz", new DateTime(2025, 2, 5, 12, 0, 0, 0, DateTimeKind.Unspecified), "İkonik basketbol tarzı", 5m, null, true, false, 3299.99m, "Nike Air Force 1", 75 },
                    { 21, "40,41,42,43,44", "Adidas", 7, "Mavi", new DateTime(2025, 2, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), "Klasik spor ayakkabı", 10m, null, true, false, 2499.99m, "Adidas Gazelle", 60 },
                    { 22, "36,37,38,39,40", "Hotiç", 5, "Altın", new DateTime(2025, 2, 6, 15, 0, 0, 0, DateTimeKind.Unspecified), "Gece ve davet için", 18m, null, true, false, 2299.99m, "Hotiç Abiye Topuklu", 40 },
                    { 23, "36,37,38,39,40,41", "Skechers", 6, "Gri", new DateTime(2025, 2, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), "Destekli taban teknolojisi", null, null, true, false, 1899.99m, "Skechers Arch Fit", 72 },
                    { 24, "39,40,41,42,43,44", "Puma", 9, "Kahverengi", new DateTime(2025, 2, 7, 11, 0, 0, 0, DateTimeKind.Unspecified), "Retro sneaker", 15m, null, true, false, 1599.99m, "Puma Suede Classic", 98 },
                    { 25, "36,37,38,39,40,41", "New Balance", 4, "Bej", new DateTime(2025, 2, 8, 10, 0, 0, 0, DateTimeKind.Unspecified), "Unisex günlük spor", 7m, null, true, false, 2399.99m, "New Balance 327", 82 },
                    { 26, "40,41,42,43,44", "Altınyıldız", 8, "Bordö", new DateTime(2025, 2, 8, 14, 0, 0, 0, DateTimeKind.Unspecified), "İş ve günlük loafer", 20m, null, true, false, 1199.99m, "Altınyıldız Loafer", 68 },
                    { 27, "28,29,30,31,32,33,34,35", "Kinetix", 12, "Yeşil", new DateTime(2025, 2, 9, 9, 0, 0, 0, DateTimeKind.Unspecified), "Okul sonrası rahat ayakkabı", 10m, null, true, false, 549.99m, "Kinetix Çocuk Günlük", 150 },
                    { 28, "28,29,30,31,32,33,34", "Polaris", 10, "Turuncu", new DateTime(2025, 2, 9, 12, 0, 0, 0, DateTimeKind.Unspecified), "Hafif ve nefes alan", null, null, true, false, 449.99m, "Polaris Çocuk Spor", 110 },
                    { 29, "40,41,42,43,44,45", "Nike", 7, "Beyaz", new DateTime(2025, 2, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), "Vintage basketbol tarzı", 12m, null, true, false, 2699.99m, "Nike Blazer Mid", 58 },
                    { 30, "36,37,38,39,40,41,42,43,44", "Converse", 9, "Siyah", new DateTime(2025, 2, 10, 15, 0, 0, 0, DateTimeKind.Unspecified), "Platform tabanlı Converse", 5m, null, true, false, 2299.99m, "Converse Run Star", 78 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApplicationUserId",
                table: "Orders",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId1",
                table: "ProductImages",
                column: "ProductId1");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId1",
                table: "ProductVariants",
                column: "ProductId1");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_ApplicationUserId",
                table: "ShoppingCarts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_ProductId",
                table: "ShoppingCarts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_ProductVariantId",
                table: "ShoppingCarts",
                column: "ProductVariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ShoppingCarts");

            migrationBuilder.DropTable(
                name: "Sliders");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
