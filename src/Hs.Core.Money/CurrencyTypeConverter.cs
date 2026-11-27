using System.ComponentModel;
using System.Globalization;

namespace Hs.Core.Money;

/// <summary>
/// Converts a <see cref="Currency"/> to and from text: its code for ISO 4217 currencies ("EUR"),
/// or its code and namespace otherwise ("BTC;virtual").
/// </summary>
public sealed class CurrencyTypeConverter : TypeConverter
{
    private const char Separator = ';';

    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    /// <exception cref="ArgumentException">The text isn't a known currency.</exception>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? text.Split(Separator, 2, StringSplitOptions.TrimEntries) switch
            {
                [var code, { Length: > 0 } @namespace] => Currency.FromCode(code, @namespace),
                [var code, ..] => Currency.FromCode(code),
                _ => Currency.FromCode(text),
            }
            : base.ConvertFrom(context, culture, value);

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
        value is Currency currency && destinationType == typeof(string)
            ? currency.Namespace == Currency.IsoNamespace ? currency.Code : $"{currency.Code}{Separator}{currency.Namespace}"
            : base.ConvertTo(context, culture, value, destinationType);
}
