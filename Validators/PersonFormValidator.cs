using FluentValidation;
using MovieRental.ViewModels.People;

namespace MovieRental.Validators;

public class PersonFormValidator : AbstractValidator<PersonFormViewModel>
{
    public PersonFormValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s.'-]+$")
            .WithMessage("Name can only contain letters, spaces, periods, hyphens, and apostrophes");

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Birth date cannot be in the future")
            .GreaterThan(new DateTime(1850, 1, 1))
            .WithMessage("Birth date seems invalid")
            .When(x => x.BirthDate.HasValue);

        RuleFor(x => x.Bio)
            .MaximumLength(2000).WithMessage("Biography cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}