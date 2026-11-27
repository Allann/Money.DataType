namespace Hs.Core.Money;

/// <summary>Thrown when an operation is given amounts in a <see cref="Currency"/> it can't work with, e.g. adding euros to dollars.</summary>
public sealed class InvalidCurrencyException : InvalidOperationException
{
    /// <summary>Initializes a new instance with a default message.</summary>
    public InvalidCurrencyException()
        : base("The requested operation cannot be performed with the specified currency.")
    {
    }

    /// <summary>Initializes a new instance with the given message.</summary>
    public InvalidCurrencyException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the given message and the exception that caused it.</summary>
    public InvalidCurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance describing the currency that was expected and the one that was given.</summary>
    public InvalidCurrencyException(Currency expected, Currency actual)
        : this($"The requested operation expected the currency {expected.Code}, but the actual value was the currency {actual.Code}.")
    {
    }
}
