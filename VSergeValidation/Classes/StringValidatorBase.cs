using VSergeValidation.Interfaces;

namespace VSergeValidation.Classes;

public abstract class StringValidatorBase : IStringValidator
{
    protected readonly int _maxLength = 128;

    public StringValidatorBase() { }
    public StringValidatorBase(int maxLength) =>
        _maxLength = maxLength;
    public abstract bool Validate(string? value, out string errorMessage);

    public bool ValidateObject(object? value, out string errorMessage)
    {
        // If null is passed, we pass it to the main method as a null string.
        if (value == null)
        {
            return Validate(null, out errorMessage);
        }

        // If the type matches (String), call the main method
        if (value is string stringValue)
        {
            return Validate(stringValue, out errorMessage);
        }

        // If it is a different data type (e.g., a number or a date), try converting it into a string.
        try
        {
            string? convertedString = Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture);
            return Validate(convertedString, out errorMessage);
        }
        catch
        {
            errorMessage = "Invalid data type for text validation.";
            return false;
        }
    }
}
