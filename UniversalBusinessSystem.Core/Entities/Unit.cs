using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("Units")]
public class Unit
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public UnitType UnitType { get; set; } = UnitType.Quantity;
    
    [Required]
    public decimal BaseConversionFactor { get; set; } = 1.0m;
    
    public Guid? BaseUnitId { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [Required]
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    [ForeignKey("BaseUnitId")]
    public virtual Unit? BaseUnit { get; set; }
    
    public virtual ICollection<OrganizationUnit> OrganizationUnits { get; set; } = new List<OrganizationUnit>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public enum UnitType
{
    Quantity = 1,
    Weight = 2,
    Length = 3,
    Volume = 4,
    Area = 5,
    Time = 6,
    Custom = 99
}
