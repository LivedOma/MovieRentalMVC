using System.ComponentModel.DataAnnotations;

namespace MovieRental.Models.Movies;

public class MovieCast
{
    [Key]
    public int MovieId { get; set; }

    [Key]
    public int PersonId { get; set; }

    [Required]
    [StringLength(150)]
    public string CharacterName { get; set; } = string.Empty;

    public int CastOrder { get; set; }

    // Navigation Properties
    public Movie Movie { get; set; } = null!;
    public Person Person { get; set; } = null!;
}