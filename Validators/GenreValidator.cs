using FluentValidation;
using MovieRental.Data;
using MovieRental.ViewModels.Genres;

namespace MovieRental.Validators;

public class GenreValidator : AbstractValidator<GenreViewModel>
{
    private readonly ApplicationDbContext _context;

    public GenreValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Genre name is required")
            .MaximumLength(50).WithMessage("Genre name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s-]+$").WithMessage("Genre name can only contain letters, spaces, and hyphens")
            .Must(BeUniqueName).WithMessage("A genre with this name already exists");
    }

    private bool BeUniqueName(GenreViewModel model, string name)
    {
        if (string.IsNullOrEmpty(name)) return true;

        var exists = _context.Genres.Any(g =>
            g.Name.ToLower() == name.ToLower() &&
            g.GenreId != model.GenreId); // Excluir el mismo registro en edición

        return !exists;
    }
}