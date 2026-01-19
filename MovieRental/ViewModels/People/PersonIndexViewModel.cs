namespace MovieRental.ViewModels.People;

public class PersonIndexViewModel
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public int MoviesAsActor { get; set; }
    public int MoviesAsCrew { get; set; }

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