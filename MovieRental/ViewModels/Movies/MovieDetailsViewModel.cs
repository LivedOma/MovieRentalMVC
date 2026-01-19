namespace MovieRental.ViewModels.Movies;

public class MovieDetailsViewModel
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Synopsis { get; set; }
    public int ReleaseYear { get; set; }
    public int DurationMinutes { get; set; }
    public string? Language { get; set; }
    public decimal RentalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<string> Genres { get; set; } = new();
    public List<CastMemberViewModel> Cast { get; set; } = new();
    public List<CrewMemberViewModel> Crew { get; set; } = new();
}

public class CastMemberViewModel
{
    public string ActorName { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public int CastOrder { get; set; }
}

public class CrewMemberViewModel
{
    public string PersonName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}