using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MovieRental.Models.Users;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation Property
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    // Computed Property (no se guarda en BD)
    public string FullName => $"{FirstName} {LastName}";
}