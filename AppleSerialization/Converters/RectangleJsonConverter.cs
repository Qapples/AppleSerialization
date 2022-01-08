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
    public class RectangleJsonConverter : JsonConverter<Rectangle>
    {
        public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            const string methodName = nameof(RectangleJsonConverter) + "." + nameof(Read);
#endif
            //first value is x, second value is y, third value is width, and last value is height.
            
            string? readerStr = reader.GetString();
            if (readerStr is null)
            {
                Debug.WriteLine($"{methodName}: unable to read Rectangle value. Using default rectangle, which " +
                                "is all zeros.");
                return new Rectangle(0, 0, 0, 0);
            }

            if (!ParseHelper.TryParseVector4(readerStr, out Vector4 value))
            {
                Debug.WriteLine($"{methodName}: unable to parse line ({readerStr}) as Vector4.");
                return new Rectangle(0, 0, 0, 0);
            }

            return new Rectangle((int) value.X, (int) value.Y, (int) value.Z, (int) value.W);
        }

        public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options) =>
            writer.WriteStringValue($"{value.X} {value.Y} {value.Width} {value.Height}");
    }
}