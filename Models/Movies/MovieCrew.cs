using System.ComponentModel.DataAnnotations;

namespace MovieRental.Models.Movies;

public class MovieCrew
{
    [Key]
    public int MovieId { get; set; }

    [Key]
    public int PersonId { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty; // Director, Writer, Producer, etc.

    // Navigation Properties
    public Movie Movie { get; set; } = null!;
    public Person Person { get; set; } = null!;
}