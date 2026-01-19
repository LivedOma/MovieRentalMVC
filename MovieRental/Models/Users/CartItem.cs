using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieRental.Models.Movies;

namespace MovieRental.Models.Users;

public class CartItem
{
    [Key]
    public int CartItemId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int MovieId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceAtAddition { get; set; } // Precio al momento de agregar

    // Navigation Properties
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;
}