using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppleSerialization.Converters
{
    public class EnumJsonConverter : JsonConverter<Enum>
    {
        public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            const string methodName = $"{nameof(EnumJsonConverter)}.{nameof(Read)}";
#endif
            string? enumString = reader.GetString();
            if (enumString is null)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: cannot read string. Returning default value for enum of type " +
                                $"{typeToConvert} ");
#endif
                return (Enum) Enum.ToObject(typeToConvert, 0);
            }

            if (!Enum.TryParse(typeToConvert, enumString, out object? enumValue))
            {
#if DEBUG
                Debug.WriteLine($"{methodName} cannot convert string value ({enumString}) to enum ({typeToConvert})");
#endif
                return (Enum) Enum.ToObject(typeToConvert, 0);
            }

            return (Enum) enumValue!; //won't be null because of TryParse check.
        }

        public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
        {
#if DEBUG
            const string methodName = $"{nameof(EnumJsonConverter)}.{nameof(Write)}";
#endif
            string enumString = value.ToString();
            writer.WriteStringValue(enumString);
        }

        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;
    }
}