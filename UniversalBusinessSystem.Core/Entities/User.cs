using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalBusinessSystem.Core.Entities;

[Table("Users")]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public bool IsEmailVerified { get; set; } = false;
    
    [MaxLength(255)]
    public string? EmailVerificationToken { get; set; }
    
    public DateTime? EmailVerificationExpires { get; set; }
    
    [Required]
    public int FailedLoginAttempts { get; set; } = 0;
    
    public DateTime? LockedUntil { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public Guid RoleId { get; set; }
    
    // Navigation properties
    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;
    
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}
