# Universal Business System (MVP) Skeleton

## Project Structure
```
UniversalBusinessSystem/
├── UniversalBusinessSystem.sln
├── src/
│   ├── UniversalBusinessSystem.App/                 # Main WPF Application
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── Views/
│   │   │   ├── LoginWindow.xaml
│   │   │   ├── RegistrationWindow.xaml
│   │   │   ├── MainWindow.xaml
│   │   │   └── Inventory/
│   │   │       ├── ProductListView.xaml
│   │   │       ├── CategoryListView.xaml
│   │   │       └── UnitListView.xaml
│   │   ├── ViewModels/
│   │   │   ├── LoginViewModel.cs
│   │   │   ├── RegistrationViewModel.cs
│   │   │   ├── MainViewModel.cs
│   │   │   └── Inventory/
│   │   │       ├── ProductListViewModel.cs
│   │   │       ├── CategoryListViewModel.cs
│   │   │       └── UnitListViewModel.cs
│   │   ├── Controls/
│   │   ├── Resources/
│   │   │   ├── Styles/
│   │   │   ├── Images/
│   │   │   └── Icons/
│   │   └── UniversalBusinessSystem.App.csproj
│   ├── UniversalBusinessSystem.Core/                # Core Business Logic
│   │   ├── Entities/
│   │   │   ├── Shop.cs
│   │   │   ├── User.cs
│   │   │   ├── Role.cs
│   │   │   ├── Permission.cs
│   │   │   ├── Unit.cs
│   │   │   ├── Product.cs
│   │   │   ├── Category.cs
│   │   │   ├── ShopType.cs
│   │   │   └── ModuleActivation.cs
│   │   ├── Services/
│   │   │   ├── IAuthenticationService.cs
│   │   │   ├── AuthenticationService.cs
│   │   │   ├── IShopService.cs
│   │   │   ├── ShopService.cs
│   │   │   ├── IInventoryService.cs
│   │   │   └── InventoryService.cs
│   │   ├── Enums/
│   │   │   ├── ShopTypeEnum.cs
│   │   │   ├── UnitTypeEnum.cs
│   │   │   └── ModuleEnum.cs
│   │   ├── Interfaces/
│   │   │   └── IModule.cs
│   │   └── UniversalBusinessSystem.Core.csproj
│   ├── UniversalBusinessSystem.Data/                 # Data Access Layer
│   │   ├── Context/
│   │   │   └── UniversalBusinessSystemDbContext.cs
│   │   ├── Migrations/
│   │   ├── Configurations/
│   │   │   ├── ShopConfiguration.cs
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── ProductConfiguration.cs
│   │   │   └── UnitConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── IRepository.cs
│   │   │   ├── Repository.cs
│   │   │   ├── IShopRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   ├── IProductRepository.cs
│   │   │   └── IUnitRepository.cs
│   │   └── UniversalBusinessSystem.Data.csproj
│   ├── UniversalBusinessSystem.Modules/              # Plugin Modules
│   │   ├── Core/
│   │   │   ├── UserManagement/
│   │   │   │   ├── UserManagementModule.cs
│   │   │   │   ├── Views/
│   │   │   │   └── ViewModels/
│   │   │   └── Inventory/
│   │   │       ├── InventoryModule.cs
│   │   │       ├── Views/
│   │   │       └── ViewModels/
│   │   ├── Interfaces/
│   │   │   └── IModule.cs
│   │   └── UniversalBusinessSystem.Modules.csproj
│   └── UniversalBusinessSystem.Infrastructure/        # Cross-cutting Concerns
│       ├── Security/
│       │   ├── PasswordHasher.cs
│       │   └── TokenProvider.cs
│       ├── Logging/
│       │   └── LoggerService.cs
│       ├── Configuration/
│       │   └── AppConfig.cs
│       └── UniversalBusinessSystem.Infrastructure.csproj
├── installer/
│   ├── UniversalBusinessSystem.iss                 # Inno Setup Script
│   ├── Resources/
│   │   ├── app-icon.ico
│   │   ├── wizard-image.bmp
│   │   └── license.txt
│   └── Output/
├── docs/
│   ├── DatabaseSchema.md
│   ├── ModuleDevelopment.md
│   └── DeploymentGuide.md
└── README.md
```

## Database Schema
```sql
-- Shop/Organization Table
CREATE TABLE Shops (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    ShopType TEXT NOT NULL,
    Address TEXT,
    Phone TEXT,
    Email TEXT,
    LicenseKey TEXT UNIQUE,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    DatabasePath TEXT
);

-- Shop Types (Pre-defined)
CREATE TABLE ShopTypes (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT,
    DefaultUnits TEXT, -- JSON array of unit IDs
    DefaultModules TEXT, -- JSON array of module keys
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    SortOrder INTEGER DEFAULT 0
);

-- Users Table
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Username TEXT NOT NULL,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    FirstName TEXT,
    LastName TEXT,
    Phone TEXT,
    ShopId TEXT NOT NULL,
    RoleId TEXT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    IsEmailVerified BOOLEAN NOT NULL DEFAULT 0,
    FailedLoginAttempts INTEGER DEFAULT 0,
    LockedUntil TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    LastLoginAt TEXT,
    FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE RESTRICT,
    UNIQUE(ShopId, Username),
    UNIQUE(ShopId, Email)
);

-- Roles Table
CREATE TABLE Roles (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    ShopId TEXT NOT NULL,
    IsSystemRole BOOLEAN NOT NULL DEFAULT 0,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE,
    UNIQUE(ShopId, Name)
);

-- Permissions Table
CREATE TABLE Permissions (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    Key TEXT NOT NULL UNIQUE,
    Description TEXT,
    Category TEXT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL
);

-- Role Permissions (Many-to-Many)
CREATE TABLE RolePermissions (
    Id TEXT PRIMARY KEY,
    RoleId TEXT NOT NULL,
    PermissionId TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    UNIQUE(RoleId, PermissionId)
);

-- Units Table
CREATE TABLE Units (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Symbol TEXT NOT NULL,
    Description TEXT,
    UnitType TEXT NOT NULL,
    BaseConversionFactor REAL NOT NULL DEFAULT 1.0,
    BaseUnitId TEXT,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    SortOrder INTEGER DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (BaseUnitId) REFERENCES Units(Id) ON DELETE RESTRICT
);

-- Shop Units (Shop-specific units)
CREATE TABLE ShopUnits (
    Id TEXT PRIMARY KEY,
    ShopId TEXT NOT NULL,
    UnitId TEXT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    SortOrder INTEGER DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE,
    FOREIGN KEY (UnitId) REFERENCES Units(Id) ON DELETE CASCADE,
    UNIQUE(ShopId, UnitId)
);

-- Categories Table
CREATE TABLE Categories (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    ParentCategoryId TEXT,
    ShopId TEXT NOT NULL,
    Color TEXT,
    Icon TEXT,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    SortOrder INTEGER DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT,
    UNIQUE(ShopId, Name)
);

-- Products Table
CREATE TABLE Products (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Sku TEXT,
    Barcode TEXT,
    Description TEXT,
    ShopId TEXT NOT NULL,
    CategoryId TEXT NOT NULL,
    UnitId TEXT NOT NULL,
    CostPrice REAL NOT NULL DEFAULT 0,
    SellingPrice REAL NOT NULL DEFAULT 0,
    DiscountPrice REAL,
    CurrentStock REAL NOT NULL DEFAULT 0,
    MinStockLevel REAL NOT NULL DEFAULT 0,
    MaxStockLevel REAL NOT NULL DEFAULT 0,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    TrackStock BOOLEAN NOT NULL DEFAULT 1,
    IsTaxable BOOLEAN NOT NULL DEFAULT 1,
    ImageUrl TEXT,
    Attributes TEXT, -- JSON for custom attributes
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    SortOrder INTEGER DEFAULT 0,
    FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UnitId) REFERENCES Units(Id) ON DELETE RESTRICT,
    UNIQUE(ShopId, Sku),
    UNIQUE(ShopId, Barcode)
);

-- Module Activations Table
CREATE TABLE ModuleActivations (
    Id TEXT PRIMARY KEY,
    ShopId TEXT NOT NULL,
    ModuleKey TEXT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    Configuration TEXT, -- JSON for module-specific config
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE,
    UNIQUE(ShopId, ModuleKey)
);

-- Settings Table
CREATE TABLE Settings (
    Id TEXT PRIMARY KEY,
    ShopId TEXT NOT NULL,
    Key TEXT NOT NULL,
    Value TEXT,
    DataType TEXT NOT NULL DEFAULT 'String', -- String, Integer, Boolean, JSON
    Category TEXT NOT NULL DEFAULT 'General',
    Description TEXT,
    IsSystem BOOLEAN NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE,
    UNIQUE(ShopId, Key)
);
```

## Core Entity Skeletons
```csharp
// UniversalBusinessSystem.Core/Entities/Shop.cs
public class Shop
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ShopTypeEnum ShopType { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LicenseKey { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? DatabasePath { get; set; }
    
    // Navigation Properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<ShopUnit> Units { get; set; } = new List<ShopUnit>();
    public virtual ICollection<ModuleActivation> ActiveModules { get; set; } = new List<ModuleActivation>();
}

// UniversalBusinessSystem.Core/Entities/User.cs
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string ShopId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation Properties
    public virtual Shop Shop { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}

// UniversalBusinessSystem.Core/Entities/Unit.cs
public class Unit
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UnitTypeEnum UnitType { get; set; }
    public decimal BaseConversionFactor { get; set; } = 1.0m;
    public string? BaseUnitId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Unit? BaseUnit { get; set; }
    public virtual ICollection<ShopUnit> ShopUnits { get; set; } = new List<ShopUnit>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

// UniversalBusinessSystem.Core/Entities/Product.cs
public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string ShopId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string UnitId { get; set; } = string.Empty;
    public decimal CostPrice { get; set; } = 0;
    public decimal SellingPrice { get; set; } = 0;
    public decimal? DiscountPrice { get; set; }
    public decimal CurrentStock { get; set; } = 0;
    public decimal MinStockLevel { get; set; } = 0;
    public decimal MaxStockLevel { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool TrackStock { get; set; } = true;
    public bool IsTaxable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string? Attributes { get; set; } // JSON for custom attributes
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int SortOrder { get; set; } = 0;
    
    // Navigation Properties
    public virtual Shop Shop { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual Unit Unit { get; set; } = null!;
}
```

## Enumerations
```csharp
// UniversalBusinessSystem.Core/Enums/ShopTypeEnum.cs
public enum ShopTypeEnum
{
    GeneralStore = 1,
    Clothing = 2,
    Electronics = 3,
    Grocery = 4,
    Mobile = 5,
    Pharmacy = 6
}

// UniversalBusinessSystem.Core/Enums/UnitTypeEnum.cs
public enum UnitTypeEnum
{
    Quantity = 1,
    Weight = 2,
    Length = 3,
    Volume = 4,
    Area = 5,
    Time = 6,
    Custom = 99
}

// UniversalBusinessSystem.Core/Enums/ModuleEnum.cs
public enum ModuleEnum
{
    UserManagement = 1,
    Inventory = 2,
    Purchases = 3,
    Sales = 4,
    Reports = 5,
    Customers = 6
}
```

## Service Interfaces
```csharp
// UniversalBusinessSystem.Core/Services/IAuthenticationService.cs
public interface IAuthenticationService
```
