using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace AppleSerialization.Converters
{
    /// <summary>
    /// Class used to convert string values to Vector2 values from Json objects
    /// </summary>
    public class Vector2JsonConverter : JsonConverter<Vector2>, IFromStringConverter
    {
        /// <summary>
        /// Given a string value from a Json value in the format of "x y" where parentheses or commas
        /// can be included and spaces unnecessary) return an instance of Vector2 that is representative of the
        /// string value. If the value is unable to be parsed, then this will return a value of 0 0
        /// </summary>
        /// <param name="reader">Utf8JsonReader instance used to read data from a Json file</param>
        /// <param name="typeToConvert">Type to convert to (Vector2)</param>
        /// <param name="options">The options of the JsonSerializer used</param>
        /// <returns>A Vector2 instance that is representative of the string value given. Returns 0 0 if unsuccessful to
        /// parse</returns>
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            const string methodName = $"{nameof(Vector2JsonConverter)}.{nameof(Read)}";
#endif
            string? vectorStr = reader.GetString();
            if (vectorStr is null)
            {
                Debug.WriteLine($"{methodName}: Unable to read Vector2 value. Using default value of (0, 0).");
                return Vector2.Zero;
            }

            return ConvertFromStringToVector(vectorStr);
        }

        /// <summary>
        /// Writes a Vector2 value to a Json object in the format of "X Y"
        /// </summary>
        /// <param name="writer">Utf8JsonWriter instance used to write to a Json object</param>
        /// <param name="value">Value to write</param>
        /// <param name="options">The options of the JsonSerializer used</param>
        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options) =>
            writer.WriteStringValue($"{value.X} {value.Y}");

        public Vector2 ConvertFromStringToVector(string vectorStr)
        {
#if DEBUG
            const string methodName = $"{nameof(Vector2JsonConverter)}.{nameof(ConvertFromStringToVector)}";
#endif
            if (!ParseHelper.TryParseVector2(vectorStr, out Vector2 vector))
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: unable to parse {vectorStr} as a Vector2. Returning default " +
                                $"value of (0,0).");
#endif
                return Vector2.Zero;
            }
            
            return vector;
        }

        public object ConvertFromString(string vectorStr) => ConvertFromStringToVector(vectorStr);
    }
}