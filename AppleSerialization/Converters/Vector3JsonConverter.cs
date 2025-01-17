using System;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace AppleSerialization.Converters
{
    /// <summary>
    /// Class used to convert string values to Vector3 values from Json objects
    /// </summary>
    public class Vector3JsonConverter : JsonConverter<Vector3>, IFromStringConverter
    {
        /// <summary>
        /// Given a string value from a Json value in the format of "x y z" where parentheses or commas
        /// can be included and spaces unnecessary) return an instance of Vector3 that is representative of the
        /// string value. If the value is unable to be parsed, then this will return a value of 0 0 0
        /// </summary>
        /// <param name="reader">Utf8JsonReader instance used to read data from a Json file</param>
        /// <param name="typeToConvert">Type to convert to (Vector3)</param>
        /// <param name="options">The options of the JsonSerializer used</param>
        /// <returns>A Vector3 instance that is representative of the string value given. Returns 0 0 0 if unsuccessful
        /// to parse</returns>
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            const string methodName = $"{nameof(Vector3JsonConverter)}.{nameof(Read)}";
            
            string? vectorStr = reader.GetString();
            if (vectorStr is null)
            {
                Debug.WriteLine($"{methodName}: Unable to read Vector3 value. Using default value of (0, 0, 0).");
                return Vector3.Zero;
            }

            return ConvertFromStringToVector(vectorStr);
        }

        /// <summary>
        /// Writes a Vector3 value to a Json object in the format of "X Y Z"
        /// </summary>
        /// <param name="writer">Utf8JsonWriter instance used to write to a Json object</param>
        /// <param name="value">Value to write</param>
        /// <param name="options">The options of the JsonSerializer used</param>
        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options) =>
            writer.WriteStringValue($"{value.X} {value.Y}");

        public Vector3 ConvertFromStringToVector(string vectorStr)
        {
#if DEBUG
            const string methodName = $"{nameof(Vector3JsonConverter)}.{nameof(ConvertFromStringToVector)}";
#endif
            if (!ParseHelper.TryParseVector3(vectorStr, out Vector3 vector))
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: unable to parse {vectorStr} as a Vector3. Returning default " +
                                $"value of (0, 0, 0).");
#endif
                return Vector3.Zero;
            }
            
            return vector;
        }

        public object ConvertFromString(string vectorStr) => ConvertFromStringToVector(vectorStr);
    }
}