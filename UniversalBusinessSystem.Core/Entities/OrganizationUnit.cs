using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("OrganizationUnits")]
public class OrganizationUnit
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public Guid UnitId { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;
    
    [ForeignKey("UnitId")]
    public virtual Unit Unit { get; set; } = null!;
}
