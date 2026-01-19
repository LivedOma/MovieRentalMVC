namespace MovieRental.ViewModels.MovieCredits;

public class ManageCreditsViewModel
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }

    public List<CastItemViewModel> Cast { get; set; } = new();
    public List<CrewItemViewModel> Crew { get; set; } = new();
}

public class CastItemViewModel
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public int CastOrder { get; set; }
}

public class CrewItemViewModel
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}