using System.ComponentModel.DataAnnotations;

namespace ContosoInventory.Shared.Validation;

/// <summary>
/// Validates that a decimal value has no more than the specified number of fractional digits.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class DecimalPlacesAttribute : ValidationAttribute
{
    private readonly int _decimalPlaces;

    /// <summary>
    /// Initializes the decimal places validator.
    /// </summary>
    public DecimalPlacesAttribute(int decimalPlaces)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        _decimalPlaces = decimalPlaces;
    }

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        return value is decimal decimalValue
            && decimal.Round(decimalValue, _decimalPlaces) == decimalValue;
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name)
    {
        return $"{name} must have no more than {_decimalPlaces} decimal places.";
    }
}