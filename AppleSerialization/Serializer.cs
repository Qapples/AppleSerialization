using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace AppleSerialization
{
    /// <summary>
    /// Abstract class that, when inherited from and/or provided a generic, grants deserialization from Json abilities
    /// to the type of the generic/subclass. Also provides a method signature for serialization, which throws an
    /// exception by default
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Given a <see cref="Utf8JsonReader"/>, deserializes an object in Json and returns an instance of type T.
        /// <br/> IMPORTANT NOTE: <see cref="Environment.DefaultFontSystem"/>,<see cref="Environment.GraphicsDevice"/>,
        /// and <see cref="Environment.ContentManager"/> CANNOT BE NULL in order for this method to function! If either
        /// are null, then this method will return null they are necessary for deserialization/serialization!
        /// </summary>
        /// <param name="reader"><see cref="Utf8JsonReader"/> instance responsible for providing Json data to
        /// deserialize.</param>
        /// <param name="options">Serialization options that changes how the data is deserialized. If null, a default
        /// value (<see cref="Environment.DefaultSerializerOptions"/>) will be used instead.</param>
        /// <returns>If deserialization is successful, an instance of type T is returned. If unsuccessful, null is
        /// returned and a debug message is displayed to the debug console.</returns>
        public static T? Deserialize<T>(ref Utf8JsonReader reader, JsonSerializerOptions? options = null)
        {
            //Ensure that the proper variables were set to in Environment
            if (Environment.DefaultFontSystem is null || Environment.GraphicsDevice is null ||
                Environment.ContentManager is null)
            {
                Debug.WriteLine("One or more of the enviornment variables are null: " +
                                $"\nDefaultFontSystem: {(Environment.DefaultFontSystem is null ? "null" : "not null")} " +
                                $"\nGraphicsDevice: {(Environment.GraphicsDevice is null ? "null" : "not null")} " +
                                $"\nContentManager: {(Environment.ContentManager is null ? "null" : "not null")}");

                //stupid null returning hack
                object? a = null;
                return (T?)a;
            }

            //this is a bit complicated, but what we are doing here is that we are using the constructor marked with
            //the JsonSerializer attribute to create an instance of T. 
            ConstructorInfo jsonConstructor = (from elm in typeof(T).GetConstructors()
                from attribute in elm.GetCustomAttributes(true)
                where attribute is JsonConstructorAttribute
                select elm).First();

            options ??= Environment.DefaultSerializerOptions;

            ParameterInfo[] jsonParameters = jsonConstructor.GetParameters();
            object?[]?
                inParameters = new object?[jsonParameters.Length]; //parameters we are going to send to the constructor

            //given the name of a parameter, return an index in inParameters
            //for example, if the first parameter is "position". Then the key "position" will return 0
            Dictionary<string, int> parameterInIndexMap = new();
            for (int i = 0; i < jsonParameters.Length; i++)
            {
                string? name = jsonParameters[i].Name;

                if (name is not null)
                {
                    parameterInIndexMap.Add(name, i);
                }
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) continue;

                string propertyName = reader.GetString()!; //PropertyNames will always be a string
                if (!reader.Read()) break; //skip to next node

                int parameterIndex = parameterInIndexMap[propertyName];
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    inParameters[parameterIndex] = ConverterHelper.GetArrayFromReader(ref reader, options);
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    inParameters[parameterIndex] = ConverterHelper.GetObjectFromReader(ref reader,
                        jsonParameters[parameterIndex].ParameterType, options);
                }
                else
                {
                    string lowerPropertyName = propertyName.ToLower();
                    object? parameterValue = ConverterHelper.GetValueFromReader(ref reader,
                        jsonParameters[parameterIndex].ParameterType, options);

                    //this is here in the instance where we are handling UI and we need to know the size before
                    //generating textures on the spot i.e. when a texture is not found and a replacement is needed.
                    //this is a pretty hacky solution. it works, I guess...
                    //TODO: find a better solution to handling size/scale for UI during serialization.
                    if (lowerPropertyName is "size" or "scale" && parameterValue is string s &&
                        ParseHelper.TryParseVector2(s, out var value))
                    {
                        Environment.CurrentDeserializingObjectSize = value;
                    }

                    inParameters[parameterIndex] = parameterValue;
                }
            }

            //if the type has a parent panel property, then give it a value
            object returnObject = jsonConstructor.Invoke(inParameters);

            return (T)returnObject;
        }
    }
}
