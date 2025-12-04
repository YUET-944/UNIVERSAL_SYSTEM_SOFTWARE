using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("Products")]
public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Sku { get; set; }
    
    [MaxLength(50)]
    public string? Barcode { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    public Guid UnitId { get; set; }
    
    [Required]
    public decimal CostPrice { get; set; } = 0;
    
    [Required]
    public decimal SellingPrice { get; set; } = 0;
    
    public decimal? DiscountPrice { get; set; }
    
    [Required]
    public decimal CurrentStock { get; set; } = 0;
    
    [Required]
    public decimal MinStockLevel { get; set; } = 0;
    
    [Required]
    public decimal MaxStockLevel { get; set; } = 0;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public bool TrackStock { get; set; } = true;
    
    [Required]
    public bool IsTaxable { get; set; } = true;
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    [Column(TypeName = "text")]
    public string? Attributes { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [Required]
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;
    
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; } = null!;
    
    [ForeignKey("UnitId")]
    public virtual Unit Unit { get; set; } = null!;
}
