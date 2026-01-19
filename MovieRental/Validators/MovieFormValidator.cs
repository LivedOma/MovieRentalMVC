using FluentValidation;
using MovieRental.Data;
using MovieRental.ViewModels.Movies;

namespace MovieRental.Validators;

public class MovieFormValidator : AbstractValidator<MovieFormViewModel>
{
    private readonly ApplicationDbContext _context;

    public MovieFormValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .Must(NotContainSpecialCharacters).WithMessage("Title contains invalid characters")
            .NotContainHtml();

        RuleFor(x => x.OriginalTitle)
            .MaximumLength(200).WithMessage("Original title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.OriginalTitle));

        RuleFor(x => x.Synopsis)
            .MaximumLength(2000).WithMessage("Synopsis cannot exceed 2000 characters")
            .NotContainHtml()
            .When(x => !string.IsNullOrEmpty(x.Synopsis));

        RuleFor(x => x.ReleaseYear)
            .NotEmpty().WithMessage("Release year is required")
            .InclusiveBetween(1888, DateTime.Now.Year + 5)
            .WithMessage($"Release year must be between 1888 and {DateTime.Now.Year + 5}");

        RuleFor(x => x.DurationMinutes)
            .NotEmpty().WithMessage("Duration is required")
            .InclusiveBetween(1, 600).WithMessage("Duration must be between 1 and 600 minutes");

        RuleFor(x => x.Language)
            .MaximumLength(50).WithMessage("Language cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Language));

        RuleFor(x => x.RentalPrice)
            .NotEmpty().WithMessage("Rental price is required")
            .GreaterThan(0).WithMessage("Rental price must be greater than $0")
            .LessThanOrEqualTo(999.99m).WithMessage("Rental price cannot exceed $999.99")
            .PrecisionScale(10, 2, false).WithMessage("Rental price can have at most 2 decimal places");

        RuleFor(x => x.SelectedGenreIds)
            .Must(ids => ids != null && ids.Count > 0)
            .WithMessage("Please select at least one genre")
            .Must(ids => ids == null || ids.Count <= 5)
            .WithMessage("A movie cannot have more than 5 genres");

        // Validación síncrona para título duplicado
        RuleFor(x => x)
            .Must(BeUniqueTitleAndYear)
            .WithMessage("A movie with this title and release year already exists")
            .WithName("Title");
    }

    private bool NotContainSpecialCharacters(string title)
    {
        if (string.IsNullOrEmpty(title)) return true;
        
        // Permitir letras, números, espacios, y algunos caracteres especiales comunes
        var invalidChars = new[] { '<', '>', '{', '}', '[', ']', '|', '\\', '^', '`' };
        return !title.Any(c => invalidChars.Contains(c));
    }

    private bool BeUniqueTitleAndYear(MovieFormViewModel model)
    {
        var existingMovie = _context.Movies.FirstOrDefault(m =>
            m.Title.ToLower() == model.Title.ToLower() &&
            m.ReleaseYear == model.ReleaseYear &&
            m.MovieId != model.MovieId); // Excluir el mismo registro en edición

        return existingMovie == null;
    }
}