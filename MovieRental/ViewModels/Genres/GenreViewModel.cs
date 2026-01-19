using System.ComponentModel.DataAnnotations;

namespace MovieRental.ViewModels.Genres;

public class GenreViewModel
{
    public int GenreId { get; set; }

    [Required(ErrorMessage = "Genre name is required")]
    [StringLength(50, ErrorMessage = "Genre name cannot exceed 50 characters")]
    [Display(Name = "Genre Name")]
    public string Name { get; set; } = string.Empty;

    public int MovieCount { get; set; }
}