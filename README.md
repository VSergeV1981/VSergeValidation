# VSergeValidation

A flexible, component-based validation engine for .NET applications. The system utilizes interfaces and the Strategy design pattern to clean isolate, encapsulate, and centrally manage validation logic for various data types including text, numbers, dates, and enumerations.

---

## Project Structure

The project is structured into two core namespaces:
* **Interfaces**: Contains the contracts governing both object-based and strongly typed validation components.
* **Classes**: Provides abstract base classes implementing boilerplate type conversions along with the central `ValidationManager`.

---

## Core Components

### 1. Interfaces

The architecture centers around `IAbstractValidator`, which natively supports untyped `object` checks while allowing type-safe generic implementations.

```csharp
namespace VSergeValidation.Interfaces;

public interface IAbstractValidator
{
    bool ValidateObject(object? value, out string errorMessage);
}

public interface IAbstractValidator<T> : IAbstractValidator
{
    bool Validate(T? value, out string errorMessage);
}
```

For specialized text-based operations, an extended interface `IStringValidator` is provided:

```csharp
namespace VSergeValidation.Interfaces;

public interface IStringValidator : IAbstractValidator<string>
{
    new bool Validate(string? value, out string errorMessage);
}
```

### 2. Abstract Base Classes

`AbstractValidatorBase<T>` intercepts incoming untyped calls via `ValidateObject`, safely evaluates type matches, and forwards execution to the type-safe `Validate` implementation.

```csharp
using VSergeValidation.Interfaces;

namespace VSergeValidation.Classes;

public abstract class AbstractValidatorBase<T> : IAbstractValidator<T>
{
    public abstract bool Validate(T? value, out string errorMessage);

    public bool ValidateObject(object? value, out string errorMessage)
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
```

`StringValidatorBase` builds upon text management by automatically translating alternative input objects (such as integers or DateTimes) into string representations using the current culture.

```csharp
using System;
using System.Globalization;
using VSergeValidation.Interfaces;

namespace VSergeValidation.Classes;

public abstract class StringValidatorBase : IStringValidator
{
    protected readonly int _maxLength = 128;

    protected StringValidatorBase() { }
    protected StringValidatorBase(int maxLength) => _maxLength = maxLength;

    public abstract bool Validate(string? value, out string errorMessage);

    public bool ValidateObject(object? value, out string errorMessage)
    {
        if (value == null)
        {
            return Validate(null, out errorMessage);
        }

        if (value is string stringValue)
        {
            return Validate(stringValue, out errorMessage);
        }

        try
        {
            string? convertedString = Convert.ToString(value, CultureInfo.CurrentCulture);
            return Validate(convertedString, out errorMessage);
        }
        catch
        {
            errorMessage = "Invalid data type for text validation.";
            return false;
        }
    }
}
```

### 3. ValidationManager

Inheriting directly from `Dictionary<string, IAbstractValidator>`, the `ValidationManager` encapsulates lookup paths to safely run checks across your operational domains.

```csharp
using System;
using System.Collections.Generic;
using VSergeValidation.Interfaces;

namespace VSergeValidation.Classes;

public sealed class ValidationManager : Dictionary<string, IAbstractValidator>
{
    public ValidationManager() : base() { }

    public ValidationManager(Dictionary<string, IAbstractValidator> validators) : base(validators) { }

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
```

---

## Production-Ready Concrete Implementation Examples

### 1. Financial Amount Validation (`decimal?`)
Validates that incoming numeric amounts are populated and strictly greater than zero.

```csharp
using VSergeValidation.Classes;

namespace WinFormsTestBankSoftware.Validation.Validators;

public sealed class BookingAmountValidator : AbstractValidatorBase<decimal?>
{
    private const string _fieldName = "Betrag";

    public override bool Validate(decimal? value, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (value is null || value.Value <= 0.00m)
        {
            errorMessage = $"Field \"{_fieldName}\" ist Pflichteingabe";
            return false;
        }
        return true;
    }
}
```

### 2. Email Address Format Validation (`string`)
Evaluates text values against uniform regular expression patterns to ensure syntax alignment.

```csharp
using System.Text.RegularExpressions;
using VSergeValidation.Classes;

namespace WinFormsTestBankSoftware.Validation.Validators;

public sealed class EMailValidator : StringValidatorBase
{
    private readonly string _eMailPattern = @"^\S+@\S+\.\S+$";
    private readonly string _fieldName = "EMail";

    public EMailValidator() : base(64) { }

    public override bool Validate(string? value, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = $"Field \"{_fieldName}\" ist Pflichteingabe";
            return false;
        }

        if (!Regex.IsMatch(value, _eMailPattern))
        {
            errorMessage = $"Field \"{_fieldName}\" hat falsches Format.";
            return false;
        }

        return true;
    }
}
```

### 3. Required General Text Validation (`string`)
A versatile text validator ensuring mandatory fields contain content without exceeding length bounds.

```csharp
using VSergeValidation.Classes;

namespace WinFormsTestBankSoftware.Validation.Validators;

public sealed class RequiredTextValidator : StringValidatorBase
{
    private readonly string _fieldName;

    public RequiredTextValidator(int maxLength, string fieldName) : base(maxLength) 
    {
        _fieldName = fieldName;
    }

    public override bool Validate(string? value, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = $"Das Feld \"{_fieldName}\" ist Pflichteingabe.";
            return false;
        }

        if (value.Length > _maxLength)
        {
            errorMessage = $"Das Feld \"{_fieldName}\" ist zu lang.";
            return false;
        }

        return true;
    }
}
```

---

## End-to-End Usage Example

### 1. Define Constant Keys (`ValidatorKeys`)
Maintain clean lookup mappings and avoid string runtime bugs by tracking validation keys inside a static class container.

```csharp
namespace WinFormsTestBankSoftware.Validation;

public static class ValidatorKeys
{
    public const string BookingAmount = "Betrag";
    public const string Email = "EMail";
    public const string FirstName = "Vorname";
}
```

<h3>2. Register and Execute Orchestration</h3>

```csharp
using System;
using VSergeValidation.Classes;
using WinFormsTestBankSoftware.Validation;
using WinFormsTestBankSoftware.Validation.Validators;

class Program
{
    static void Main()
    {
        // 1. Initialize the Validation Engine
        ValidationManager validationEngine = new ValidationManager();

        // 2. Register Concrete Implementations under Defined Mappings
        validationEngine.AddNewValidator(ValidatorKeys.BookingAmount, new BookingAmountValidator());
        validationEngine.AddNewValidator(ValidatorKeys.Email, new EMailValidator());
        validationEngine.AddNewValidator(ValidatorKeys.FirstName, new RequiredTextValidator(50, "Vorname"));

        // 3. Perform Runtime Data Inquiries
        
        // Scenario A: Decimal Validation Failure Handling
        decimal? inputAmount = 0.00m;
        if (!validationEngine.IsValid(ValidatorKeys.BookingAmount, inputAmount, out string amountError))
        {
            Console.WriteLine($"Validation Error: {amountError}"); 
            // Output: Validation Error: Field "Betrag" ist Pflichteingabe
        }

        // Scenario B: Regular Expression Match Failure Handling
        string inputEmail = "user@domaincom";
        if (!validationEngine.IsValid(ValidatorKeys.Email, inputEmail, out string emailError))
        {
            Console.WriteLine($"Validation Error: {emailError}");
            // Output: Validation Error: Field "EMail" hat falsches Format.
        }

        // Scenario C: Successful Validation Path Execution
        string inputName = "Serge";
    }
}

```
---
[![.NET Core CI/CD](https://github.com)](https://github.com)
