using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Models.Movies;

public class Movie
{
    [Key]
    public int MovieId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(200)]
    public string? OriginalTitle { get; set; }

    [StringLength(2000)]
    public string? Synopsis { get; set; }

    [Range(1888, 2100)] // 1888 = primera película de la historia
    public int ReleaseYear { get; set; }

    [Range(1, 1000)]
    public int DurationMinutes { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 9999.99)]
    public decimal RentalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MovieCast> MovieCasts { get; set; } = new List<MovieCast>();
    public ICollection<MovieCrew> MovieCrews { get; set; } = new List<MovieCrew>();
}