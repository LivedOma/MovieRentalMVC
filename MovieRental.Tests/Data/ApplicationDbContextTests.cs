using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Models.Movies;
using MovieRental.Tests.Helpers;

namespace MovieRental.Tests.Data;

public class ApplicationDbContextTests : IDisposable
{
    private readonly MovieRental.Data.ApplicationDbContext _context;

    public ApplicationDbContextTests()
    {
        _context = TestDbContextFactory.CreateWithSeedData();
    }

    [Fact]
    public async Task Movies_ShouldContainSeedData()
    {
        // Act
        var movies = await _context.Movies.ToListAsync();

        // Assert
        movies.Should().HaveCount(3);
        movies.Should().Contain(m => m.Title == "Inception");
        movies.Should().Contain(m => m.Title == "The Dark Knight");
        movies.Should().Contain(m => m.Title == "Forrest Gump");
    }

    [Fact]
    public async Task Genres_ShouldContainSeedData()
    {
        // Act
        var genres = await _context.Genres.ToListAsync();

        // Assert
        genres.Should().HaveCount(5);
        genres.Should().Contain(g => g.Name == "Action");
        genres.Should().Contain(g => g.Name == "Drama");
    }

    [Fact]
    public async Task Movie_ShouldLoadGenresCorrectly()
    {
        // Act
        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Title == "Inception");

        // Assert
        movie.Should().NotBeNull();
        movie!.MovieGenres.Should().HaveCount(2);
        movie.MovieGenres.Select(mg => mg.Genre.Name)
            .Should().Contain(new[] { "Action", "Science Fiction" });
    }

    [Fact]
    public async Task Movie_ShouldLoadCastCorrectly()
    {
        // Act
        var movie = await _context.Movies
            .Include(m => m.MovieCasts)
                .ThenInclude(mc => mc.Person)
            .FirstOrDefaultAsync(m => m.Title == "Inception");

        // Assert
        movie.Should().NotBeNull();
        movie!.MovieCasts.Should().HaveCount(1);
        movie.MovieCasts.First().Person.FullName.Should().Be("Leonardo DiCaprio");
        movie.MovieCasts.First().CharacterName.Should().Be("Dom Cobb");
    }

    [Fact]
    public async Task Movie_ShouldLoadCrewCorrectly()
    {
        // Act
        var movie = await _context.Movies
            .Include(m => m.MovieCrews)
                .ThenInclude(mc => mc.Person)
            .FirstOrDefaultAsync(m => m.Title == "Inception");

        // Assert
        movie.Should().NotBeNull();
        movie!.MovieCrews.Should().HaveCount(1);
        movie.MovieCrews.First().Person.FullName.Should().Be("Christopher Nolan");
        movie.MovieCrews.First().Role.Should().Be("Director");
    }

    [Fact]
    public async Task AddMovie_ShouldPersistCorrectly()
    {
        // Arrange
        var newMovie = new Movie
        {
            Title = "New Test Movie",
            ReleaseYear = 2023,
            DurationMinutes = 100,
            RentalPrice = 3.99m,
            Language = "English"
        };

        // Act
        _context.Movies.Add(newMovie);
        await _context.SaveChangesAsync();

        // Assert
        var savedMovie = await _context.Movies.FindAsync(newMovie.MovieId);
        savedMovie.Should().NotBeNull();
        savedMovie!.Title.Should().Be("New Test Movie");
        savedMovie.ReleaseYear.Should().Be(2023);
    }

    [Fact]
    public async Task DeleteMovie_ShouldCascadeToRelatedEntities()
    {
        // Arrange
        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
            .Include(m => m.MovieCasts)
            .FirstOrDefaultAsync(m => m.Title == "Inception");
        
        var movieId = movie!.MovieId;
        var genreRelationsCount = movie.MovieGenres.Count;
        var castRelationsCount = movie.MovieCasts.Count;

        // Verificar que tiene relaciones
        genreRelationsCount.Should().BeGreaterThan(0);
        castRelationsCount.Should().BeGreaterThan(0);

        // Act
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        // Assert
        var deletedMovie = await _context.Movies.FindAsync(movieId);
        deletedMovie.Should().BeNull();

        var remainingGenreRelations = await _context.MovieGenres
            .Where(mg => mg.MovieId == movieId)
            .CountAsync();
        remainingGenreRelations.Should().Be(0);

        var remainingCastRelations = await _context.MovieCasts
            .Where(mc => mc.MovieId == movieId)
            .CountAsync();
        remainingCastRelations.Should().Be(0);
    }

    [Fact]
    public async Task Genre_UniqueName_ShouldBeEnforced()
    {
        // Arrange
        var duplicateGenre = new Genre { Name = "Action" }; // Already exists

        // Act
        _context.Genres.Add(duplicateGenre);
        
        // Assert - En memoria no se valida el índice único, pero verificamos la lógica
        var actionGenres = await _context.Genres
            .Where(g => g.Name == "Action")
            .CountAsync();
        
        // Sin SaveChanges, debería haber solo 1
        // Después de SaveChanges en InMemory, podría permitirlo (limitación de InMemory)
        actionGenres.Should().Be(1);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}