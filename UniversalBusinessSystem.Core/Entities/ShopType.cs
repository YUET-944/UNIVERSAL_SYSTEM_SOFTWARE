using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("ShopTypes")]
public class ShopType
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    [Required]
    public int SortOrder { get; set; } = 0;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Default units for this shop type (JSON array of unit IDs)
    [Column(TypeName = "text")]
    public string? DefaultUnits { get; set; }
    
    // Default modules for this shop type (JSON array of module IDs)
    [Column(TypeName = "text")]
    public string? DefaultModules { get; set; }
    
    // Navigation properties
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
