namespace MovieRental.ViewModels.Movies;

public class MovieIndexViewModel
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public int DurationMinutes { get; set; }
    public decimal RentalPrice { get; set; }
    public List<string> Genres { get; set; } = new();
}