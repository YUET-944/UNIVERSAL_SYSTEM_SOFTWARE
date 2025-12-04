using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("Permissions")]
public class Permission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Key { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public PermissionCategory Category { get; set; } = PermissionCategory.General;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public enum PermissionCategory
{
    General = 1,
    Users = 2,
    Products = 3,
    Categories = 4,
    Inventory = 5,
    Sales = 6,
    Purchases = 7,
    Reports = 8,
    Settings = 9,
    Modules = 10
}
