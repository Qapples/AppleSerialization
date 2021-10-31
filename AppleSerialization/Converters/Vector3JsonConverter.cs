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
                Debug.WriteLine($"{methodName}: Unable to READ Vector3 value. Using default value of (0, 0, 0)");
                return Vector3.Zero;
            }

            //ignore spaces and parentheses by only taking the actual values into consideration and ignoring unneeded
            //values
            if (!ParseHelper.TryParseVector3(readerStr, out Vector3 value))
            {
                Debug.WriteLine($"{methodName}: cannot parse vector3 value! Using default value of (0, 0, 0)");
                return Vector3.Zero;
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}