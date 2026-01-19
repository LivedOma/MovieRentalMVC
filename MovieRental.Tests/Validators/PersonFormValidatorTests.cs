using FluentValidation.TestHelper;
using MovieRental.Validators;
using MovieRental.ViewModels.People;

namespace MovieRental.Tests.Validators;

public class PersonFormValidatorTests
{
    private readonly PersonFormValidator _validator;

    public PersonFormValidatorTests()
    {
        _validator = new PersonFormValidator();
    }

    [Fact]
    public void Validate_WithValidPerson_ShouldNotHaveErrors()
    {
        // Arrange
        var model = new PersonFormViewModel
        {
            FullName = "John Doe",
            BirthDate = new DateTime(1990, 5, 15),
            Bio = "A talented actor."
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var model = new PersonFormViewModel { FullName = "" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("Full name is required");
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveError()
    {
        // Arrange
        var model = new PersonFormViewModel { FullName = new string('A', 151) };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("Full name cannot exceed 150 characters");
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("Jane@Doe")]
    [InlineData("Bob#Smith")]
    public void Validate_WithInvalidNameCharacters_ShouldHaveError(string name)
    {
        // Arrange
        var model = new PersonFormViewModel { FullName = name };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("John O'Brien")]
    [InlineData("Mary-Jane Watson")]
    [InlineData("José García")]
    [InlineData("François Müller")]
    public void Validate_WithValidSpecialCharactersInName_ShouldNotHaveError(string name)
    {
        // Arrange
        var model = new PersonFormViewModel { FullName = name };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WithFutureBirthDate_ShouldHaveError()
    {
        // Arrange
        var model = new PersonFormViewModel
        {
            FullName = "John Doe",
            BirthDate = DateTime.Today.AddDays(1)
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
            .WithErrorMessage("Birth date cannot be in the future");
    }

    [Fact]
    public void Validate_WithVeryOldBirthDate_ShouldHaveError()
    {
        // Arrange
        var model = new PersonFormViewModel
        {
            FullName = "John Doe",
            BirthDate = new DateTime(1800, 1, 1)
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
            .WithErrorMessage("Birth date seems invalid");
    }

    [Fact]
    public void Validate_WithNullBirthDate_ShouldNotHaveError()
    {
        // Arrange
        var model = new PersonFormViewModel
        {
            FullName = "John Doe",
            BirthDate = null
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Validate_WithBioTooLong_ShouldHaveError()
    {
        // Arrange
        var model = new PersonFormViewModel
        {
            FullName = "John Doe",
            Bio = new string('A', 2001)
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Bio)
            .WithErrorMessage("Biography cannot exceed 2000 characters");
    }
}