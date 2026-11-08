using VSergeValidation.Interfaces;

namespace VSergeValidation.Classes;

public abstract class AbstractValidatorBase<T> : IAbstractValidator<T>
{
    public abstract bool Validate(T? value, out string errorMessage);

    bool IAbstractValidator.ValidateObject(object? value, out string errorMessage)
    {
        if (value == null)
        {
            return Validate(default, out errorMessage);
        }

        if (value is T typedValue)
        {
            return Validate(typedValue, out errorMessage);
        }


        errorMessage = "Invalid data type for validation.";
        return false;
    }
}
