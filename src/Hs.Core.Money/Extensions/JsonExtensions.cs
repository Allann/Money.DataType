using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hs.Core.Money.Extensions;

/// <summary>
/// Extension methods used for Json Serialization
/// </summary>
public static class JsonExtensions
{
    private static JsonSerializerOptions GetSettings(bool includeNull = true)
    {
        return new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = includeNull
                ? JsonIgnoreCondition.Never
                : JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Serialize object into a valid JSON string using System.Text.Json.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object being serialized</param>
    /// <returns>Json string representation of the object</returns>
    public static string ToJson<T>(this T obj)
    {
        return ToJson(obj, true);
    }

    /// <summary>
    /// Serialize object into a valid JSON string using System.Text.Json.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object being serialized</param>
    /// <param name="includeNull">Flag to determine if null properties will be serialized</param>
    /// <returns>Json string representation of the object</returns>
    public static string ToJson<T>(this T obj, bool includeNull)
    {
        return JsonSerializer.Serialize(obj, GetSettings(includeNull));
    }

    /// <summary>
    /// Deserialize a valid JSON string into its object type.
    /// </summary>
    /// <typeparam name="T">Type of object to deserialize</typeparam>
    /// <param name="json">Json string representing the object</param>
    /// <returns>Object of the type specified</returns>
    public static T? FromJson<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, GetSettings());
    }
}
