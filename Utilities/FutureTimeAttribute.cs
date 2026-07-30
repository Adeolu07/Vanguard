using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Utilities;

public class FutureTimeAttribute : ValidationAttribute
{
    private readonly int _minimumMinutesAhead;

    public FutureTimeAttribute(int minimumMinutesAhead = 30)
    {
        _minimumMinutesAhead = minimumMinutesAhead;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not DateTime dateTime)
            return new ValidationResult("Invalid date.");

        var earliest = DateTime.Now.AddMinutes(_minimumMinutesAhead);

        if (dateTime < earliest)
            return new ValidationResult(
                $"Trip must be scheduled at least {_minimumMinutesAhead} minutes from now.");

        return ValidationResult.Success;
    }
}