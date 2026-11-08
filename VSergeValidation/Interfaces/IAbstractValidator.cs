namespace VSergeValidation.Interfaces;

public interface IAbstractValidator
{
    bool ValidateObject(object? value, out string errorMessage);
}


public interface IAbstractValidator<T> : IAbstractValidator
{
    bool Validate(T? value, out string errorMessage);
}
