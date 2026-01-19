using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Movies;

namespace MovieRental.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static ApplicationDbContext CreateWithSeedData()
    {
        var context = Create();
        SeedTestData(context);
        return context;
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Géneros
        var genres = new List<Genre>
        {
            new() { GenreId = 1, Name = "Action" },
            new() { GenreId = 2, Name = "Drama" },
            new() { GenreId = 3, Name = "Comedy" },
            new() { GenreId = 4, Name = "Science Fiction" },
            new() { GenreId = 5, Name = "Horror" }
        };
        context.Genres.AddRange(genres);

        // Personas
        var people = new List<Person>
        {
            new() { PersonId = 1, FullName = "Christopher Nolan", BirthDate = new DateTime(1970, 7, 30) },
            new() { PersonId = 2, FullName = "Leonardo DiCaprio", BirthDate = new DateTime(1974, 11, 11) },
            new() { PersonId = 3, FullName = "Tom Hanks", BirthDate = new DateTime(1956, 7, 9) }
        };
        context.People.AddRange(people);

        // Películas
        var movies = new List<Movie>
        {
            new()
            {
                MovieId = 1,
                Title = "Inception",
                OriginalTitle = "Inception",
                ReleaseYear = 2010,
                DurationMinutes = 148,
                Language = "English",
                RentalPrice = 4.99m,
                Synopsis = "A thief who enters the dreams of others.",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                MovieId = 2,
                Title = "The Dark Knight",
                OriginalTitle = "The Dark Knight",
                ReleaseYear = 2008,
                DurationMinutes = 152,
                Language = "English",
                RentalPrice = 3.99m,
                Synopsis = "Batman faces the Joker.",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                MovieId = 3,
                Title = "Forrest Gump",
                OriginalTitle = "Forrest Gump",
                ReleaseYear = 1994,
                DurationMinutes = 142,
                Language = "English",
                RentalPrice = 2.99m,
                Synopsis = "Life is like a box of chocolates.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
        context.Movies.AddRange(movies);

        // MovieGenres
        var movieGenres = new List<MovieGenre>
        {
            new() { MovieId = 1, GenreId = 1 }, // Inception - Action
            new() { MovieId = 1, GenreId = 4 }, // Inception - Sci-Fi
            new() { MovieId = 2, GenreId = 1 }, // Dark Knight - Action
            new() { MovieId = 2, GenreId = 2 }, // Dark Knight - Drama
            new() { MovieId = 3, GenreId = 2 }, // Forrest Gump - Drama
            new() { MovieId = 3, GenreId = 3 }  // Forrest Gump - Comedy
        };
        context.MovieGenres.AddRange(movieGenres);

        // MovieCast
        var movieCasts = new List<MovieCast>
        {
            new() { MovieId = 1, PersonId = 2, CharacterName = "Dom Cobb", CastOrder = 1 },
            new() { MovieId = 3, PersonId = 3, CharacterName = "Forrest Gump", CastOrder = 1 }
        };
        context.MovieCasts.AddRange(movieCasts);

        // MovieCrew
        var movieCrews = new List<MovieCrew>
        {
            new() { MovieId = 1, PersonId = 1, Role = "Director" },
            new() { MovieId = 2, PersonId = 1, Role = "Director" }
        };
        context.MovieCrews.AddRange(movieCrews);

        context.SaveChanges();
    }
}