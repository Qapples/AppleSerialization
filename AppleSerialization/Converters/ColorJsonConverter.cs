using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace AppleSerialization.Converters
{
    /// <summary>
    /// Class used to convert string values found in Json files to Color instances
    /// </summary>
    public class ColorJsonConverter : JsonConverter<Color>, IFromStringConverter
    {
        /// <summary>
        /// From the name of a color ("green", "red", etc.) in a Json context, return the appropriate Color struct value
        /// If the value provided by the JsonReader is null, then the default value returned will be Color.Transparent
        /// If the name is invalid, the default value will also be Color.Transparent.
        /// If you do not want to use the static const values provided by the Color class, then explicitly set the
        /// R, G, B, A values.
        /// </summary>
        /// <param name="reader">Reader struct provided to read the value</param>
        /// <param name="typeToConvert">The type value that is being converted (should be Color)</param>
        /// <param name="options">The options of the JsonSerializer used</param>
        /// <returns>The appropriate Color value based on the name given</returns>
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            const string methodName = $"{nameof(ColorJsonConverter)}.{nameof(Read)}";

            string? colorStr = reader.GetString();
            if (colorStr is null)
            {
                Debug.WriteLine($"{methodName}: Unable to obtain string value in reading Color. Using default " +
                                $"Color.Transparent value.");
                return Color.Transparent;
            }

            return ConvertFromStringToColor(colorStr);
        }

        /// <summary>
        /// Given a color value, write the appropriate Json string using RGBA values in the format of "{R} {G} {B} {A}"
        /// </summary>
        /// <param name="writer">Object used to write the Json </param>
        /// <param name="value">The color value to write </param>
        /// <param name="options">The options of the JsonSerializer used</param>
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) =>
            writer.WriteStringValue($"{value.R} {value.G} {value.B} {value.A}");

        private static Color ConvertFromStringToColor(string colorStr)
        {
            const string methodName = $"{nameof(ColorJsonConverter)}.{nameof(ConvertFromStringToColor)}";

            Color? color = TextureHelper.GetColorFromName(colorStr);
            if (color is not null) return color.Value;
            
            if (!ParseHelper.TryParseColor(colorStr, out Color parsedColor))
            {
                Debug.WriteLine($"{methodName} :Unable to parse color string value ({colorStr}). Either the " +
                                $"string is in an improper format or one of the values is outside the accepted " +
                                $"range [0, 255]. Using default Color.Transparent value.");
            }

            return parsedColor;
        }

        public object ConvertFromString(string colorStr) => ConvertFromStringToColor(colorStr);
    }
}