using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace AppleSerialization.Converters
{
    /// <summary>
    /// Class used to convert data from Json files into <see cref="Rectangle"/> instances.
    /// </summary>
    public class RectangleJsonConverter : JsonConverter<Rectangle>, IFromStringConverter
    {
        public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            const string methodName = nameof(RectangleJsonConverter) + "." + nameof(Read);
#endif
            //first value is x, second value is y, third value is width, and last value is height.
            string? rectStr = reader.GetString();
            if (rectStr is null)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: unable to read Rectangle value. Using default rectangle, which " +
                                "is all zeros.");
#endif
                return new Rectangle();
            }

            return ConvertFromStringToRect(rectStr);
        }

        public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options) =>
            writer.WriteStringValue($"{value.X} {value.Y} {value.Width} {value.Height}");

        public Rectangle ConvertFromStringToRect(string rectStr)
        {
#if DEBUG
            const string methodName = nameof(RectangleJsonConverter) + "." + nameof(ConvertFromStringToRect);
#endif
            //first value is x, second value is y, third value is width, and last value is height.
            if (!ParseHelper.TryParseVector4(rectStr, out Vector4 value))
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: unable to parse line ({rectStr}) as Vector4.");
#endif
                return new Rectangle();
            }

            return new Rectangle((int) value.X, (int) value.Y, (int) value.Z, (int) value.W);
        }

        public object ConvertFromString(string rectStr) => ConvertFromStringToRect(rectStr);
    }
}