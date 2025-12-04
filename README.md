# Universal Business System

A professional, modular Windows desktop application for business management with dynamic shop type configuration and module-based architecture.

## Features

### Core Features
- **Multi-tenant Architecture**: Support multiple organizations with isolated data
- **Dynamic Shop Types**: Configure for different business types (General Store, Clothing, Electronics, Grocery, Mobile, Pharmacy)
- **Modular System**: Enable/disable features per organization
- **User Management**: Role-based access control with permissions
- **Inventory Management**: Products, categories, and units management
- **Modern WPF UI**: Material Design-inspired interface

### Shop Type Support
- **General Store**: Basic inventory and sales
- **Clothing/Boutique**: Size/color variants management
- **Electronics**: Warranty and serial number tracking
- **Grocery**: Weight-based pricing and expiry tracking
- **Mobile Shop**: IMEI tracking and device management
- **Pharmacy**: Batch and expiry date management

### Technical Features
- **Local SQLite Database**: Offline-first with cloud migration path
- **Secure Authentication**: BCrypt password hashing, account lockout
- **Error Logging**: Comprehensive logging with Serilog
- **Auto-updater**: Built-in update mechanism
- **Digital Signature Ready**: Prepared for code signing

## System Requirements

- **Operating System**: Windows 8.1 or later
- **Framework**: .NET 7.0 Runtime
- **Memory**: 4GB RAM minimum
- **Storage**: 500MB free space
- **Processor**: x64 architecture

## Installation

1. Download the installer from the releases page
2. Run `UniversalBusinessSystem-Setup-1.0.0.exe`
3. Follow the installation wizard
4. Launch the application from Desktop or Start Menu

### Quick Start

1. **First Time Setup**: Create a new account during registration
2. **Select Business Type**: Choose your shop type during registration
3. **Configure Units**: Select measurement units relevant to your business
4. **Activate Modules**: Enable modules based on your needs
5. **Add Products**: Start adding your inventory

## Default Login

For testing purposes, use the default administrator account:
- **Username**: `admin`
- **Password**: `admin123`

## Project Structure

```
UniversalBusinessSystem/
├── UniversalBusinessSystem.sln          # Solution file
├── UniversalBusinessSystem/              # Main WPF Application
│   ├── Views/                            # UI Views
│   ├── ViewModels/                       # MVVM ViewModels
│   ├── App.xaml                          # Application entry point
│   └── Resources/                        # Icons and resources
├── UniversalBusinessSystem.Core/         # Core Business Logic
│   ├── Entities/                         # Database Entities
│   └── Services/                         # Business Services
├── UniversalBusinessSystem.Data/          # Data Access Layer
│   ├── UniversalBusinessSystemDbContext.cs
│   └── DatabaseService.cs
├── UniversalBusinessSystem.Modules/      # Plugin Modules
│   ├── UserManagement/
│   ├── Inventory/
│   └── [Industry-specific modules]
└── Installer/                            # Inno Setup Scripts
    └── UniversalBusinessSystem.iss
```

## Database Schema

### Core Tables
- **Organizations**: Multi-tenant organization data
- **Users**: User accounts with role assignments
- **Roles**: Role definitions with permissions
- **Permissions**: Granular permission system
- **ShopTypes**: Business type configurations
- **Units**: Measurement units and conversions
- **Modules**: Available system modules
- **OrganizationModules**: Active modules per organization
- **Products**: Product catalog with flexible attributes
- **Categories**: Product categorization system

### Key Features
- **JSON Fields**: Flexible product attributes using JSON storage
- **Multi-tenancy**: All tables include OrganizationId for data isolation
- **Audit Trail**: CreatedAt, UpdatedAt timestamps
- **Soft Deletes**: IsActive flags for data retention

## Module System

The application uses a plugin-based architecture:

### Core Modules
- **User Management**: User accounts, roles, and permissions
- **Inventory Management**: Products, categories, and stock management

### Industry-Specific Modules
- **Size/Color Variants**: For clothing and fashion
- **Warranty Management**: For electronics and appliances
- **Weight-based Pricing**: For grocery stores
- **IMEI Tracking**: For mobile device shops
- **Batch & Expiry**: For pharmacies and consumables

### Module Activation
Modules can be activated/deactivated per organization through the Module Management interface.

## Development

### Prerequisites
- Visual Studio 2022 or later
- .NET 7.0 SDK
- Entity Framework Core 7.0
- Inno Setup (for installer creation)

### Building the Application
```bash
# Clone the repository
git clone [repository-url]
cd UniversalBusinessSystem

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run the application
dotnet run --project UniversalBusinessSystem
```

### Creating the Installer
1. Install Inno Setup 6.x
2. Open `Installer/UniversalBusinessSystem.iss`
3. Build the installer using Inno Setup compiler

### Database Migration
```bash
# Add new migration
dotnet ef migrations add MigrationName --project UniversalBusinessSystem.Data

# Update database
dotnet ef database update --project UniversalBusinessSystem.Data
```

## Configuration

### Application Settings
Configuration is stored in `%AppData%\UniversalBusinessSystem\`:
- Database file location
- User preferences
- Module configurations
- Log files

### Database Configuration
- **Default Location**: `%AppData%\UniversalBusinessSystem\UniversalBusinessSystem.db`
- **Backup Location**: Application folder `\backups\`
- **Migration Path**: Ready for cloud database migration

## Security

### Authentication
- **Password Hashing**: BCrypt with salt
- **Account Lockout**: 5 failed attempts triggers 30-minute lockout
- **Session Management**: Secure token-based sessions

### Data Protection
- **Local Encryption**: SQLite database encryption optional
- **Access Control**: Role-based permissions system
- **Audit Logging**: User action tracking

## Support

### Documentation
- User Guide: Available in the application Help menu
- Developer Documentation: See `/docs` folder
- API Documentation: Available for module developers

### Troubleshooting
- **Logs**: Check `%AppData%\UniversalBusinessSystem\logs\`
- **Database**: Verify database file permissions
- **Modules**: Ensure required modules are activated

## Roadmap

### Version 1.1 (Planned)
- Cloud synchronization
- Advanced reporting
- Mobile app companion
- Multi-language support

### Version 1.2 (Planned)
- E-commerce integration
- Advanced analytics
- API for third-party integrations
- Enhanced security features

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Credits

- **Universal Software Solutions** - Development team
- **Material Design in XAML** - UI framework
- **Entity Framework Core** - ORM framework
- **Serilog** - Logging framework
- **Inno Setup** - Installer framework
