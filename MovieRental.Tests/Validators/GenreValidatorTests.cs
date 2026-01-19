using FluentAssertions;
using FluentValidation.TestHelper;
using MovieRental.Data;
using MovieRental.Tests.Helpers;
using MovieRental.Validators;
using MovieRental.ViewModels.Genres;

namespace MovieRental.Tests.Validators;

public class GenreValidatorTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GenreValidator _validator;

    public GenreValidatorTests()
    {
        _context = TestDbContextFactory.CreateWithSeedData();
        _validator = new GenreValidator(_context);
    }

    [Fact]
    public async Task Validate_WithValidGenre_ShouldNotHaveErrors()
    {
        // Arrange
        var model = new GenreViewModel { Name = "Musical" };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var model = new GenreViewModel { Name = "" };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Genre name is required");
    }

    [Fact]
    public async Task Validate_WithNameTooLong_ShouldHaveError()
    {
        // Arrange
        var model = new GenreViewModel { Name = new string('A', 51) };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Genre name cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("Action123")]
    [InlineData("Drama@#$")]
    [InlineData("Comedy!")]
    public async Task Validate_WithInvalidCharacters_ShouldHaveError(string name)
    {
        // Arrange
        var model = new GenreViewModel { Name = name };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Genre name can only contain letters, spaces, and hyphens");
    }

    [Fact]
    public async Task Validate_WithDuplicateName_ShouldHaveError()
    {
        // Arrange - "Action" already exists in seed data
        var model = new GenreViewModel { GenreId = 0, Name = "Action" };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A genre with this name already exists");
    }

    [Fact]
    public async Task Validate_WithDuplicateNameCaseInsensitive_ShouldHaveError()
    {
        // Arrange
        var model = new GenreViewModel { GenreId = 0, Name = "ACTION" };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A genre with this name already exists");
    }

    [Fact]
    public async Task Validate_WhenEditingSameGenre_ShouldNotHaveDuplicateError()
    {
        // Arrange - GenreId = 1 is "Action"
        var model = new GenreViewModel { GenreId = 1, Name = "Action" };

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