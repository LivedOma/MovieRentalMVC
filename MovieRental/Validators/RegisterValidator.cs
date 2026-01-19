using FluentValidation;
using Microsoft.AspNetCore.Identity;
using MovieRental.Models.Users;
using MovieRental.ViewModels.Account;

namespace MovieRental.Validators;

public class RegisterValidator : AbstractValidator<RegisterViewModel>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s'-]+$")
            .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s'-]+$")
            .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please enter a valid email address")
            .Must(BeUniqueEmail).WithMessage("This email is already registered");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your password")
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }

    private bool BeUniqueEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return true;
        
        var existingUser = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
        return existingUser == null;
    }
}