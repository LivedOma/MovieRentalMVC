using System.ComponentModel.DataAnnotations;

namespace MovieRental.Models.Movies;

public class Person
{
    [Key]
    public int PersonId { get; set; }

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [StringLength(2000)]
    public string? Bio { get; set; }

    // Navigation Properties
    public ICollection<MovieCast> MovieCasts { get; set; } = new List<MovieCast>();
    public ICollection<MovieCrew> MovieCrews { get; set; } = new List<MovieCrew>();
}