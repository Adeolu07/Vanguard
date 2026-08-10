using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace _Tripfinity.Utilities;

public class FutureTimeAttribute : ValidationAttribute, IClientModelValidator
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

        // The model binder gives us Unspecified; treat it as local
        var incoming = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        var earliest = DateTime.Now.AddMinutes(_minimumMinutesAhead);

        if (incoming < earliest)
            return new ValidationResult(
                $"Trip must be scheduled at least {_minimumMinutesAhead} minutes from now.");

        return ValidationResult.Success;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        var earliest = DateTime.Now.AddMinutes(_minimumMinutesAhead);
        // Format for datetime-local without seconds or milliseconds
        var min = earliest.ToString("yyyy-MM-ddTHH:mm");
        context.Attributes["min"] = min;
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-futuretime"] =
            $"Trip must be scheduled at least {_minimumMinutesAhead} minutes from now.";
    }
}