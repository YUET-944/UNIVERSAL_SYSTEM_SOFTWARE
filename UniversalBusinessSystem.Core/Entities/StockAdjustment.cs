using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("StockAdjustments")]
public class StockAdjustment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid AdjustedBy { get; set; }

    [Required]
    [MaxLength(50)]
    public string AdjustmentType { get; set; } = "Manual";

    public int QuantityChange { get; set; }

    public decimal UnitCost { get; set; }

    [MaxLength(200)]
    public string? Reason { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey(nameof(AdjustedBy))]
    public virtual User User { get; set; } = null!;
}
