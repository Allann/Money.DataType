using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hs.Core.Money.Extensions;

/// <summary>JSON helpers using System.Text.Json with camelCase names and case-insensitive reading.</summary>
public static class JsonExtensions
{
    // Options are immutable once used, and building them per call allocates and re-checks the metadata cache; share them.
    private static readonly JsonSerializerOptions WithNulls = Options(JsonIgnoreCondition.Never);
    private static readonly JsonSerializerOptions WithoutNulls = Options(JsonIgnoreCondition.WhenWritingNull);

    extension<T>(T obj)
    {
        /// <summary>Serializes <paramref name="obj"/> to JSON, including null properties.</summary>
        public string ToJson() => obj.ToJson(includeNull: true);

        /// <summary>Serializes <paramref name="obj"/> to JSON, leaving out null properties unless <paramref name="includeNull"/>.</summary>
        public string ToJson(bool includeNull) => JsonSerializer.Serialize(obj, includeNull ? WithNulls : WithoutNulls);
    }

    extension(string json)
    {
        /// <summary>Deserializes JSON into a <typeparamref name="T"/>.</summary>
        /// <exception cref="JsonException">The JSON is invalid or doesn't match <typeparamref name="T"/>.</exception>
        public T? FromJson<T>() => JsonSerializer.Deserialize<T>(json, WithNulls);
    }

    private static JsonSerializerOptions Options(JsonIgnoreCondition ignore) => new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = ignore,
    };
}
