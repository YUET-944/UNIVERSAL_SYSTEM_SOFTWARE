using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("OrganizationModules")]
public class OrganizationModule
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public Guid ModuleId { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [Column(TypeName = "text")]
    public string? Configuration { get; set; }
    
    // Navigation properties
    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;
    
    [ForeignKey("ModuleId")]
    public virtual Module Module { get; set; } = null!;
}
