using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace AppleSerialization.Converters
{
    public class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            const string methodName = nameof(Vector3JsonConverter) + "." + nameof(Read);
            
            string? readerStr = reader.GetString();
            if (readerStr is null)
            {
                Debug.WriteLine("Unable to READ Vector3 value. Using default value of (0, 0, 0)");
                return Vector3.Zero;
            }

            //ignore spaces and parentheses by only taking the actual values into consideration and ignoring unneeded
            //values
            var (hasX, hasY, hasZ) = (ConverterHelper.GetNumFromStrVector(in readerStr, 0, out float x, out int i),
                ConverterHelper.GetNumFromStrVector(in readerStr, i, out float y, out i),
                ConverterHelper.GetNumFromStrVector(in readerStr, i, out float z, out i));

            if (!hasX || !hasY || !hasZ)
            {
                Debug.WriteLine($"{methodName}: unable to parse a value. Returning a default value of (0, 0, 0). " +
                                $"Can't parse these values: {(!hasX ? "X" : "")} {(!hasY ? "Y" : "")} {(!hasZ ? "Z" : "")}");

                return Vector3.Zero;
            }

            return new Vector3(x, y, z);
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}