using FluentAssertions;
using FluentValidation.TestHelper;
using MovieRental.Data;
using MovieRental.Tests.Helpers;
using MovieRental.Validators;
using MovieRental.ViewModels.Movies;

namespace MovieRental.Tests.Validators;

public class MovieFormValidatorTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MovieFormValidator _validator;

    public MovieFormValidatorTests()
    {
        _context = TestDbContextFactory.CreateWithSeedData();
        _validator = new MovieFormValidator(_context);
    }

    [Fact]
    public async Task Validate_WithValidMovie_ShouldNotHaveErrors()
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = "New Movie",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int> { 1, 2 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = "",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int> { 1 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public async Task Validate_WithTitleTooLong_ShouldHaveError()
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = new string('A', 201), // 201 characters
            ReleaseYear = 2023,
            DurationMinutes = 120,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int> { 1 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData(1887)]
    [InlineData(2035)]
    public async Task Validate_WithInvalidYear_ShouldHaveError(int year)
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = "Test Movie",
            ReleaseYear = year,
            DurationMinutes = 120,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int> { 1 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReleaseYear);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(601)]
    public async Task Validate_WithInvalidDuration_ShouldHaveError(int duration)
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = "Test Movie",
            ReleaseYear = 2023,
            DurationMinutes = duration,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int> { 1 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WithInvalidPrice_ShouldHaveError(decimal price)
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = "Test Movie",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            RentalPrice = price,
            SelectedGenreIds = new List<int> { 1 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RentalPrice);
    }

    [Fact]
    public async Task Validate_WithNoGenres_ShouldHaveError()
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = "Test Movie",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int>()
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SelectedGenreIds)
            .WithErrorMessage("Please select at least one genre");
    }

    [Fact]
    public async Task Validate_WithTooManyGenres_ShouldHaveError()
    {
        // Arrange
        var model = new MovieFormViewModel
        {
            Title = "Test Movie",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int> { 1, 2, 3, 4, 5, 6 } // 6 genres
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SelectedGenreIds)
            .WithErrorMessage("A movie cannot have more than 5 genres");
    }

    [Fact]
    public async Task Validate_WithDuplicateTitleAndYear_ShouldHaveError()
    {
        // Arrange - "Inception" 2010 already exists in seed data
        var model = new MovieFormViewModel
        {
            MovieId = 0, // New movie
            Title = "Inception",
            ReleaseYear = 2010,
            DurationMinutes = 120,
            RentalPrice = 4.99m,
            SelectedGenreIds = new List<int> { 1 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage("A movie with this title and release year already exists");
    }

    [Fact]
    public async Task Validate_WhenEditingSameMovie_ShouldNotHaveDuplicateError()
    {
        // Arrange - Editing existing movie (MovieId = 1 is Inception 2010)
        var model = new MovieFormViewModel
        {
            MovieId = 1, // Same movie ID
            Title = "Inception",
            ReleaseYear = 2010,
            DurationMinutes = 148,
            RentalPrice = 5.99m,
            SelectedGenreIds = new List<int> { 1 }
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}