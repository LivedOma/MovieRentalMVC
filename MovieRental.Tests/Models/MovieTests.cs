using FluentAssertions;
using MovieRental.Models.Movies;

namespace MovieRental.Tests.Models;

public class MovieTests
{
    [Fact]
    public void Movie_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var movie = new Movie();

        // Assert
        movie.MovieId.Should().Be(0);
        movie.Title.Should().BeEmpty();
        movie.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        movie.MovieGenres.Should().NotBeNull().And.BeEmpty();
        movie.MovieCasts.Should().NotBeNull().And.BeEmpty();
        movie.MovieCrews.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Movie_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var movie = new Movie
        {
            MovieId = 1,
            Title = "Test Movie",
            OriginalTitle = "Original Test Movie",
            Synopsis = "A test movie synopsis",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            Language = "English",
            RentalPrice = 4.99m
        };

        // Assert
        movie.MovieId.Should().Be(1);
        movie.Title.Should().Be("Test Movie");
        movie.OriginalTitle.Should().Be("Original Test Movie");
        movie.Synopsis.Should().Be("A test movie synopsis");
        movie.ReleaseYear.Should().Be(2023);
        movie.DurationMinutes.Should().Be(120);
        movie.Language.Should().Be("English");
        movie.RentalPrice.Should().Be(4.99m);
    }

    [Theory]
    [InlineData(1888)]
    [InlineData(2000)]
    [InlineData(2024)]
    public void Movie_ReleaseYear_ShouldAcceptValidYears(int year)
    {
        // Arrange & Act
        var movie = new Movie { ReleaseYear = year };

        // Assert
        movie.ReleaseYear.Should().Be(year);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(4.99)]
    [InlineData(999.99)]
    public void Movie_RentalPrice_ShouldAcceptValidPrices(decimal price)
    {
        // Arrange & Act
        var movie = new Movie { RentalPrice = price };

        // Assert
        movie.RentalPrice.Should().Be(price);
    }
}