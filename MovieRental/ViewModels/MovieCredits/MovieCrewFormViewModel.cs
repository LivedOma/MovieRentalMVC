using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieRental.ViewModels.MovieCredits;

public class MovieCrewFormViewModel
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a person")]
    [Display(Name = "Person")]
    public int PersonId { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    // Para los dropdowns
    public SelectList? AvailablePeople { get; set; }
    public SelectList? AvailableRoles { get; set; }

    // Roles predefinidos
    public static List<string> PredefinedRoles => new()
    {
        "Director",
        "Writer",
        "Producer",
        "Executive Producer",
        "Cinematographer",
        "Editor",
        "Composer",
        "Production Designer",
        "Costume Designer"
    };
}