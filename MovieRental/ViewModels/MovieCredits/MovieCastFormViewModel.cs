using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieRental.ViewModels.MovieCredits;

public class MovieCastFormViewModel
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a person")]
    [Display(Name = "Actor")]
    public int PersonId { get; set; }

    [Required(ErrorMessage = "Character name is required")]
    [StringLength(150, ErrorMessage = "Character name cannot exceed 150 characters")]
    [Display(Name = "Character Name")]
    public string CharacterName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cast order is required")]
    [Range(1, 100, ErrorMessage = "Cast order must be between 1 and 100")]
    [Display(Name = "Billing Order")]
    public int CastOrder { get; set; } = 1;

    // Para el dropdown
    public SelectList? AvailablePeople { get; set; }
}