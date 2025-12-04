using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("PurchaseOrderItems")]
public class PurchaseOrderItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PurchaseOrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal LineTotal { get; set; }

    public decimal ReceivedQuantity { get; set; } = 0;

    [ForeignKey(nameof(PurchaseOrderId))]
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}
