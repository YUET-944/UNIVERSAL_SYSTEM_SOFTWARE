using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("Modules")]
public class Module
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Key { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    [Required]
    public ModuleType ModuleType { get; set; } = ModuleType.Core;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public bool IsRequired { get; set; } = false;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [Required]
    public int SortOrder { get; set; } = 0;
    
    [Required]
    public string AssemblyName { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? EntryPointClass { get; set; }
    
    // Navigation properties
    public virtual ICollection<OrganizationModule> OrganizationModules { get; set; } = new List<OrganizationModule>();
}

public enum ModuleType
{
    Core = 1,
    Inventory = 2,
    Sales = 3,
    Purchasing = 4,
    Reporting = 5,
    Advanced = 6,
    Industry = 7
}
