using System.ComponentModel.DataAnnotations;

namespace MovieRental.Models.Movies;

public class MovieGenre
{
    [Key]
    public int MovieId { get; set; }

    [Key]
    public int GenreId { get; set; }

    // Navigation Properties
    public Movie Movie { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}