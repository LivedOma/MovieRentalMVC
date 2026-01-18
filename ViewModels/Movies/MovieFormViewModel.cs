using System.ComponentModel.DataAnnotations;

namespace MovieRental.ViewModels.Movies;

public class MovieFormViewModel
{
    public int MovieId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Original title cannot exceed 200 characters")]
    [Display(Name = "Original Title")]
    public string? OriginalTitle { get; set; }

    [StringLength(2000, ErrorMessage = "Synopsis cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    public string? Synopsis { get; set; }

    [Required(ErrorMessage = "Release year is required")]
    [Range(1888, 2100, ErrorMessage = "Release year must be between 1888 and 2100")]
    [Display(Name = "Release Year")]
    public int ReleaseYear { get; set; }

    [Required(ErrorMessage = "Duration is required")]
    [Range(1, 1000, ErrorMessage = "Duration must be between 1 and 1000 minutes")]
    [Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [Required(ErrorMessage = "Rental price is required")]
    [Range(0.01, 9999.99, ErrorMessage = "Rental price must be between $0.01 and $9,999.99")]
    [DataType(DataType.Currency)]
    [Display(Name = "Rental Price")]
    public decimal RentalPrice { get; set; }

    // Para el formulario de selección de géneros
    [Display(Name = "Genres")]
    public List<int> SelectedGenreIds { get; set; } = new();
    public List<GenreCheckboxViewModel> AvailableGenres { get; set; } = new();
}

public class GenreCheckboxViewModel
{
    public int GenreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}