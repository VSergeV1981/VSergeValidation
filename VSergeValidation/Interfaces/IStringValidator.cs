namespace VSergeValidation.Interfaces;

public interface IStringValidator : IAbstractValidator<string>
{
    new bool Validate(string? value, out string errorMessage);
}

