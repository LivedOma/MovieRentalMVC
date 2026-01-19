using System.ComponentModel.DataAnnotations;

namespace MovieRental.ViewModels.People;

public class PersonFormViewModel
{
    public int PersonId { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Birth Date")]
    public DateTime? BirthDate { get; set; }

    [StringLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    public string? Bio { get; set; }
}