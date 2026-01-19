using FluentValidation;
using MovieRental.Data;
using MovieRental.ViewModels.MovieCredits;

namespace MovieRental.Validators;

public class MovieCastValidator : AbstractValidator<MovieCastFormViewModel>
{
    private readonly ApplicationDbContext _context;

    public MovieCastValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Please select an actor")
            .Must(PersonExists).WithMessage("Selected person does not exist");

        RuleFor(x => x.CharacterName)
            .NotEmpty().WithMessage("Character name is required")
            .MaximumLength(150).WithMessage("Character name cannot exceed 150 characters");

        RuleFor(x => x.CastOrder)
            .InclusiveBetween(1, 100).WithMessage("Billing order must be between 1 and 100");

        RuleFor(x => x)
            .Must(BeUniqueCastEntry)
            .WithMessage("This person is already in the cast of this movie")
            .WithName("PersonId");
    }

    private bool PersonExists(int personId)
    {
        return _context.People.Any(p => p.PersonId == personId);
    }

    private bool BeUniqueCastEntry(MovieCastFormViewModel model)
    {
        return !_context.MovieCasts.Any(mc =>
            mc.MovieId == model.MovieId &&
            mc.PersonId == model.PersonId);
    }
}