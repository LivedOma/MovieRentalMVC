using System.ComponentModel.DataAnnotations;

namespace MovieRental.Models.Movies;

public class Genre
{
    [Key]
    public int GenreId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    // Navigation Property
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}