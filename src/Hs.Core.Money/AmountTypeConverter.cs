using System.ComponentModel;
using System.Globalization;

namespace Hs.Core.Money;

/// <summary>
/// Converts an <see cref="Amount"/> to and from text as its value and ISO 4217 code, e.g. "1234.56 EUR",
/// with the number in the given culture (invariant when none is given).
/// </summary>
public sealed class AmountTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    /// <exception cref="FormatException">The text isn't a number followed by a currency code.</exception>
    /// <exception cref="ArgumentException">The currency code isn't known.</exception>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) switch
            {
                [var number, var code] => new Amount(decimal.Parse(number, NumberStyles.Number, culture ?? CultureInfo.InvariantCulture), code),
                _ => throw new FormatException($"'{text}' is not an amount followed by a currency code, such as '1234.56 EUR'."),
            }
            : base.ConvertFrom(context, culture, value);

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
        value is Amount amount && destinationType == typeof(string)
            ? $"{amount.Value.ToString(culture ?? CultureInfo.InvariantCulture)} {amount.Currency.Code}"
            : base.ConvertTo(context, culture, value, destinationType);
}
