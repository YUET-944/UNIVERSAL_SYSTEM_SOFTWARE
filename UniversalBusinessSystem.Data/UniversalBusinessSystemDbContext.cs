using Microsoft.EntityFrameworkCore;
using UniversalBusinessSystem.Core.Entities;

namespace UniversalBusinessSystem.Data;

public class UniversalBusinessSystemDbContext : DbContext
{
    public UniversalBusinessSystemDbContext(DbContextOptions<UniversalBusinessSystemDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<ShopType> ShopTypes { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<OrganizationModule> OrganizationModules { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<StockAdjustment> StockAdjustments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Organization
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.LicenseKey).HasMaxLength(50);
            entity.Property(e => e.DatabasePath).HasMaxLength(100);
            entity.HasIndex(e => e.LicenseKey).IsUnique();
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.EmailVerificationToken).HasMaxLength(255);
            entity.HasIndex(e => new { e.OrganizationId, e.Username }).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Email }).IsUnique();
        });

        // Configure Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.HasIndex(e => new { e.OrganizationId, e.Name }).IsUnique();
        });

        // Configure Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // Configure RolePermission (many-to-many)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });

        // Configure ShopType
        modelBuilder.Entity<ShopType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.DefaultUnits).HasColumnType("text");
            entity.Property(e => e.DefaultModules).HasColumnType("text");
        });

        // Configure Unit
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.HasOne(e => e.BaseUnit)
                .WithMany()
                .HasForeignKey(e => e.BaseUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrganizationUnit
        modelBuilder.Entity<OrganizationUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Units)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Unit)
                .WithMany(u => u.OrganizationUnits)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.OrganizationId, e.UnitId }).IsUnique();
        });

        // Configure Module
        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.AssemblyName).IsRequired();
            entity.Property(e => e.EntryPointClass).HasMaxLength(255);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // Configure OrganizationModule
        modelBuilder.Entity<OrganizationModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Configuration).HasColumnType("text");
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.ActiveModules)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Module)
                .WithMany(m => m.OrganizationModules)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.OrganizationId, e.ModuleId }).IsUnique();
        });

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Categories)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.OrganizationId, e.Name }).IsUnique();
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Attributes).HasColumnType("text");
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPrice).HasPrecision(18, 2);
            entity.Property(e => e.CurrentStock).HasPrecision(18, 4);
            entity.Property(e => e.MinStockLevel).HasPrecision(18, 4);
            entity.Property(e => e.MaxStockLevel).HasPrecision(18, 4);
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Products)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Unit)
                .WithMany(u => u.Products)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.OrganizationId, e.Sku }).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Barcode }).IsUnique();
        });

        // Configure Supplier
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrganizationId, e.Code }).IsUnique();
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.Name);

            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.Balance).HasPrecision(18, 2);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Suppliers)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PurchaseOrder
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrganizationId, e.OrderNumber }).IsUnique();
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.PurchaseOrders)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PurchaseOrderItem
        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Quantity).HasPrecision(18, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.Property(e => e.ReceivedQuantity).HasPrecision(18, 3);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure StockAdjustment
        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.AdjustedBy);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.AdjustmentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(200);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.StockAdjustments)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.AdjustedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Shop Types
        var shopTypes = new[]
        {
            new ShopType { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "General Store", Description = "General retail store", SortOrder = 1, DefaultUnits = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\"]", DefaultModules = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\"]" },
            new ShopType { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), Name = "Clothing/Boutique", Description = "Fashion and clothing store", SortOrder = 2, DefaultUnits = "[\"11111111-1111-1111-1111-111111111111\"]", DefaultModules = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\",\"33333333-3333-3333-3333-333333333333\"]" },
            new ShopType { Id = Guid.Parse("11111111-1111-1111-1111-111111111113"), Name = "Electronics", Description = "Electronics and appliances", SortOrder = 3, DefaultUnits = "[\"11111111-1111-1111-1111-111111111111\"]", DefaultModules = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\",\"44444444-4444-4444-4444-444444444444\"]" },
            new ShopType { Id = Guid.Parse("11111111-1111-1111-1111-111111111114"), Name = "Grocery", Description = "Grocery and food items", SortOrder = 4, DefaultUnits = "[\"22222222-2222-2222-2222-222222222222\",\"33333333-3333-3333-3333-333333333333\"]", DefaultModules = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\",\"55555555-5555-5555-5555-555555555555\"]" },
            new ShopType { Id = Guid.Parse("11111111-1111-1111-1111-111111111115"), Name = "Mobile Shop", Description = "Mobile phones and accessories", SortOrder = 5, DefaultUnits = "[\"11111111-1111-1111-1111-111111111111\"]", DefaultModules = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\",\"66666666-6666-6666-6666-666666666666\"]" },
            new ShopType { Id = Guid.Parse("11111111-1111-1111-1111-111111111116"), Name = "Pharmacy", Description = "Pharmacy and medical supplies", SortOrder = 6, DefaultUnits = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\"]", DefaultModules = "[\"11111111-1111-1111-1111-111111111111\",\"22222222-2222-2222-2222-222222222222\",\"77777777-7777-7777-7777-777777777777\"]" }
        };
        modelBuilder.Entity<ShopType>().HasData(shopTypes);

        // Seed Units
        var units = new[]
        {
            new Unit { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Piece", Symbol = "pcs", UnitType = UnitType.Quantity, BaseConversionFactor = 1m, SortOrder = 1 },
            new Unit { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Kilogram", Symbol = "kg", UnitType = UnitType.Weight, BaseConversionFactor = 1m, SortOrder = 2 },
            new Unit { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Liter", Symbol = "L", UnitType = UnitType.Volume, BaseConversionFactor = 1m, SortOrder = 3 },
            new Unit { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Meter", Symbol = "m", UnitType = UnitType.Length, BaseConversionFactor = 1m, SortOrder = 4 },
            new Unit { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Gram", Symbol = "g", UnitType = UnitType.Weight, BaseConversionFactor = 0.001m, BaseUnitId = Guid.Parse("22222222-2222-2222-2222-222222222222"), SortOrder = 5 },
            new Unit { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Milliliter", Symbol = "mL", UnitType = UnitType.Volume, BaseConversionFactor = 0.001m, BaseUnitId = Guid.Parse("33333333-3333-3333-3333-333333333333"), SortOrder = 6 },
            new Unit { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Box", Symbol = "box", UnitType = UnitType.Quantity, BaseConversionFactor = 1m, SortOrder = 7 },
            new Unit { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Dozen", Symbol = "doz", UnitType = UnitType.Quantity, BaseConversionFactor = 12m, SortOrder = 8 }
        };
        modelBuilder.Entity<Unit>().HasData(units);

        // Seed Modules
        var modules = new[]
        {
            new Module { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "User Management", Key = "user_management", Description = "Manage users and roles", ModuleType = ModuleType.Core, IsRequired = true, AssemblyName = "UniversalBusinessSystem.Modules", EntryPointClass = "UniversalBusinessSystem.Modules.UserManagement.UserManagementModule", SortOrder = 1 },
            new Module { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Inventory Management", Key = "inventory_management", Description = "Manage products and categories", ModuleType = ModuleType.Core, IsRequired = true, AssemblyName = "UniversalBusinessSystem.Modules", EntryPointClass = "UniversalBusinessSystem.Modules.Inventory.InventoryModule", SortOrder = 2 },
            new Module { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Size/Color Variants", Key = "size_color_variants", Description = "Manage product variants for clothing", ModuleType = ModuleType.Industry, AssemblyName = "UniversalBusinessSystem.Modules", EntryPointClass = "UniversalBusinessSystem.Modules.Variants.VariantsModule", SortOrder = 3 },
            new Module { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Warranty Management", Key = "warranty_management", Description = "Manage product warranties", ModuleType = ModuleType.Industry, AssemblyName = "UniversalBusinessSystem.Modules", EntryPointClass = "UniversalBusinessSystem.Modules.Warranty.WarrantyModule", SortOrder = 4 },
            new Module { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Weight-based Pricing", Key = "weight_pricing", Description = "Weight-based pricing for groceries", ModuleType = ModuleType.Industry, AssemblyName = "UniversalBusinessSystem.Modules", EntryPointClass = "UniversalBusinessSystem.Modules.WeightPricing.WeightPricingModule", SortOrder = 5 },
            new Module { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "IMEI Tracking", Key = "imei_tracking", Description = "Track IMEI numbers for mobile devices", ModuleType = ModuleType.Industry, AssemblyName = "UniversalBusinessSystem.Modules", EntryPointClass = "UniversalBusinessSystem.Modules.IMEI.IMEIModule", SortOrder = 6 },
            new Module { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Batch & Expiry", Key = "batch_expiry", Description = "Track batch numbers and expiry dates", ModuleType = ModuleType.Industry, AssemblyName = "UniversalBusinessSystem.Modules", EntryPointClass = "UniversalBusinessSystem.Modules.BatchExpiry.BatchExpiryModule", SortOrder = 7 }
        };
        modelBuilder.Entity<Module>().HasData(modules);

        // Seed Permissions
        var permissions = new[]
        {
            // General permissions
            new Permission { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "View Dashboard", Key = "dashboard.view", Description = "View main dashboard", Category = PermissionCategory.General },
            new Permission { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab"), Name = "Access Settings", Key = "settings.access", Description = "Access application settings", Category = PermissionCategory.Settings },
            
            // User management permissions
            new Permission { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "View Users", Key = "users.view", Description = "View user list", Category = PermissionCategory.Users },
            new Permission { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc"), Name = "Create Users", Key = "users.create", Description = "Create new users", Category = PermissionCategory.Users },
            new Permission { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbd"), Name = "Edit Users", Key = "users.edit", Description = "Edit existing users", Category = PermissionCategory.Users },
            new Permission { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbe"), Name = "Delete Users", Key = "users.delete", Description = "Delete users", Category = PermissionCategory.Users },
            new Permission { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbf"), Name = "Manage Roles", Key = "roles.manage", Description = "Manage user roles and permissions", Category = PermissionCategory.Users },
            
            // Product management permissions
            new Permission { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "View Products", Key = "products.view", Description = "View product list", Category = PermissionCategory.Products },
            new Permission { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccd"), Name = "Create Products", Key = "products.create", Description = "Create new products", Category = PermissionCategory.Products },
            new Permission { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccce"), Name = "Edit Products", Key = "products.edit", Description = "Edit existing products", Category = PermissionCategory.Products },
            new Permission { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccf"), Name = "Delete Products", Key = "products.delete", Description = "Delete products", Category = PermissionCategory.Products },
            
            // Category management permissions
            new Permission { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd0"), Name = "View Categories", Key = "categories.view", Description = "View category list", Category = PermissionCategory.Categories },
            new Permission { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd1"), Name = "Create Categories", Key = "categories.create", Description = "Create new categories", Category = PermissionCategory.Categories },
            new Permission { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd2"), Name = "Edit Categories", Key = "categories.edit", Description = "Edit existing categories", Category = PermissionCategory.Categories },
            new Permission { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd3"), Name = "Delete Categories", Key = "categories.delete", Description = "Delete categories", Category = PermissionCategory.Categories },
            
            // Module management permissions
            new Permission { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "View Modules", Key = "modules.view", Description = "View available modules", Category = PermissionCategory.Modules },
            new Permission { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeef"), Name = "Manage Modules", Key = "modules.manage", Description = "Activate/deactivate modules", Category = PermissionCategory.Modules }
        };
        modelBuilder.Entity<Permission>().HasData(permissions);
    }
}
