using FluentValidation;
using MovieRental.Data;
using MovieRental.ViewModels.MovieCredits;

namespace MovieRental.Validators;

public class MovieCrewValidator : AbstractValidator<MovieCrewFormViewModel>
{
    private readonly ApplicationDbContext _context;

    public MovieCrewValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Please select a person")
            .Must(PersonExists).WithMessage("Selected person does not exist");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .MaximumLength(50).WithMessage("Role cannot exceed 50 characters");

        RuleFor(x => x)
            .Must(BeUniqueCrewEntry)
            .WithMessage("This person already has this role in this movie")
            .WithName("Role");
    }

    private bool PersonExists(int personId)
    {
        return _context.People.Any(p => p.PersonId == personId);
    }

    private bool BeUniqueCrewEntry(MovieCrewFormViewModel model)
    {
        return !_context.MovieCrews.Any(mc =>
            mc.MovieId == model.MovieId &&
            mc.PersonId == model.PersonId &&
            mc.Role == model.Role);
    }
}