using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hs.Core.Money;

/// <summary>Converts <see cref="Amount"/> values to and from JSON.</summary>
public sealed class AmountJsonConverter : JsonConverter<Amount>
{
    /// <inheritdoc />
    public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("An amount must be represented by a JSON object.");
        }

        decimal? value = null;
        string? currency = null;
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name.Equals("value", StringComparison.OrdinalIgnoreCase))
            {
                value = ReadAmountValue(property.Value);
            }
            else if (property.Name.Equals("currency", StringComparison.OrdinalIgnoreCase))
            {
                currency = ReadAmountCurrency(property.Value);
            }
        }

        if (currency is null)
        {
            throw new ArgumentNullException("currency", "An amount must include a currency.");
        }

        if (value is null)
        {
            throw new JsonException("An amount must include a value.");
        }

        return new Amount(value.Value, currency);
    }

    private static decimal ReadAmountValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(
                element.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var number) => number,
            _ => throw new JsonException("An amount value must be a decimal number or numeric string.")
        };
    }

    private static string? ReadAmountCurrency(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Object => ReadCurrencyCode(element),
            _ => throw new JsonException("An amount currency must be a currency code.")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Amount value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("currency", value.Currency.Code);
        writer.WriteEndObject();
    }

    /// <summary>Reads the currency code from a serialized currency object.</summary>
    /// <param name="element">The JSON object containing the currency code.</param>
    /// <returns>The currency code.</returns>
    internal static string ReadCurrencyCode(JsonElement element)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (!property.Name.Equals("code", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value.ValueKind != JsonValueKind.String || property.Value.GetString() is not { } code)
            {
                throw new JsonException("A currency code must be a string.");
            }

            return code;
        }

        throw new JsonException("A currency object must include a code.");
    }
}

/// <summary>Converts <see cref="Currency"/> values to and from JSON.</summary>
public sealed class CurrencyJsonConverter : JsonConverter<Currency>
{
    /// <inheritdoc />
    public override Currency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var code = root.ValueKind switch
        {
            JsonValueKind.String => root.GetString(),
            JsonValueKind.Object => AmountJsonConverter.ReadCurrencyCode(root),
            _ => throw new JsonException("A currency must be represented by a currency code.")
        };

        if (code is null)
        {
            throw new JsonException("A currency code cannot be null.");
        }

        return Currency.FromCode(code);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Code);
    }
}
