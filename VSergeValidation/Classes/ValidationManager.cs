using VSergeValidation.Interfaces;

namespace VSergeValidation.Classes;

public sealed class ValidationManager : Dictionary<string, IAbstractValidator>
{

    public ValidationManager() : base()
    {
    }

    public ValidationManager(Dictionary<string, IAbstractValidator> valiodators) : base(valiodators)
    {
    }

    public void AddValidatorCollections(Dictionary<string, IAbstractValidator> validator)
    {
        base.Clear();
        foreach (var kvp in validator)
        {
            base.Add(kvp.Key, kvp.Value);
        }
    }

    public void AddNewValidator(string key, IAbstractValidator validator) => base.Add(key, validator);

    public bool IsValid<T>(string validatorKey, T? value, out string errorMessage)
    {
        if (!this.TryGetValue(validatorKey, out var validator))
        {
            throw new InvalidOperationException($"Validator \"{validatorKey}\" ist nicht vorhanden.");
        }

        return validator.ValidateObject(value, out errorMessage);
    }
}
