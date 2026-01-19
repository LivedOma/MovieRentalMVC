namespace MovieRental.ViewModels.People;

public class PersonDetailsViewModel
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string? Bio { get; set; }

    public List<MovieRoleViewModel> MoviesAsActor { get; set; } = new();
    public List<MovieCrewRoleViewModel> MoviesAsCrew { get; set; } = new();

    public int? Age
    {
        get
        {
            if (!BirthDate.HasValue) return null;
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Value.Year;
            if (BirthDate.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}

public class MovieRoleViewModel
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public int CastOrder { get; set; }
}

public class MovieCrewRoleViewModel
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string Role { get; set; } = string.Empty;
}