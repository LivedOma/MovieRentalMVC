using FluentValidation;

namespace MovieRental.Validators;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> NotContainHtml<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => string.IsNullOrEmpty(value) || !ContainsHtml(value))
            .WithMessage("HTML tags are not allowed");
    }

    public static IRuleBuilderOptions<T, string> NotContainProfanity<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => string.IsNullOrEmpty(value) || !ContainsProfanity(value))
            .WithMessage("Content contains inappropriate language");
    }

    public static IRuleBuilderOptions<T, decimal> ValidCurrency<T>(
        this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .PrecisionScale(10, 2, false).WithMessage("Amount can have at most 2 decimal places");
    }

    private static bool ContainsHtml(string value)
    {
        return value.Contains('<') && value.Contains('>');
    }

    private static bool ContainsProfanity(string value)
    {
        // Lista básica de ejemplo - en producción usar una librería especializada
        var profanityList = new[] { "badword1", "badword2" }; // Ejemplo
        var lowerValue = value.ToLower();
        return profanityList.Any(p => lowerValue.Contains(p));
    }
}