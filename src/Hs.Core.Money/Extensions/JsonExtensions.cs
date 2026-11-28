using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hs.Core.Money.Extensions;

/// <summary>JSON helpers using System.Text.Json with camelCase names and case-insensitive reading.</summary>
public static class JsonExtensions
{
    // Options are immutable once used, and building them per call allocates and re-checks the metadata cache; share them.
    private static readonly JsonSerializerOptions WithNulls = Options(JsonIgnoreCondition.Never);
    private static readonly JsonSerializerOptions WithoutNulls = Options(JsonIgnoreCondition.WhenWritingNull);

    /// <summary>Serializes <paramref name="obj"/> to JSON, including null properties.</summary>
    public static string ToJson<T>(this T obj) => ToJson(obj, includeNull: true);

    /// <summary>Serializes <paramref name="obj"/> to JSON, leaving out null properties unless <paramref name="includeNull"/>.</summary>
    public static string ToJson<T>(this T obj, bool includeNull) => JsonSerializer.Serialize(obj, includeNull ? WithNulls : WithoutNulls);

    /// <summary>Deserializes JSON into a <typeparamref name="T"/>.</summary>
    /// <exception cref="JsonException">The JSON is invalid or doesn't match <typeparamref name="T"/>.</exception>
    public static T? FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, WithNulls);

    private static JsonSerializerOptions Options(JsonIgnoreCondition ignore) => new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = ignore,
    };
}
