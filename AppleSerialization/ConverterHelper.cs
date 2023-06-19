using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppleSerialization.Converters;
using AppleSerialization.Info;
using AppleSerialization.Json;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JsonProperty = AppleSerialization.Json.JsonProperty;

namespace AppleSerialization
{
    /// <summary>
    /// Static class that provides methods that assist in working with serialization and deserialization
    /// </summary>
    public static class ConverterHelper
    {
        /// <summary>
        /// All types that are not converted in the TypeConverterMap are listed here
        /// </summary>
        // we're not using the in type reference to match the name of the methods of the Utf8JsonReader struct
        // .GetBoolean(), .GetByte(), .GetSingle(), etc.
        private static readonly Dictionary<Type, GetDelegate> ExcludedTypes = new()
        {
            { typeof(Boolean), (ref Utf8JsonReader reader) => reader.GetBoolean() },
            { typeof(Byte), (ref Utf8JsonReader reader) => reader.GetByte() },
            { typeof(DateTime), (ref Utf8JsonReader reader) => reader.GetDateTime() },
            { typeof(Decimal), (ref Utf8JsonReader reader) => reader.GetDecimal() },
            { typeof(Double), (ref Utf8JsonReader reader) => reader.GetDouble() },
            { typeof(Guid), (ref Utf8JsonReader reader) => reader.GetGuid() },
            { typeof(Int16), (ref Utf8JsonReader reader) => reader.GetInt16() },
            { typeof(Int32), (ref Utf8JsonReader reader) => reader.GetInt32() },
            { typeof(Int64), (ref Utf8JsonReader reader) => reader.GetInt64() },
            { typeof(SByte), (ref Utf8JsonReader reader) => reader.GetSByte() },
            { typeof(Single), (ref Utf8JsonReader reader) => reader.GetSingle() },
            { typeof(String), (ref Utf8JsonReader reader) => reader.GetString() },
            { typeof(UInt16), (ref Utf8JsonReader reader) => reader.GetUInt16() },
            { typeof(UInt32), (ref Utf8JsonReader reader) => reader.GetUInt32() },
            { typeof(UInt64), (ref Utf8JsonReader reader) => reader.GetUInt64() }
        };

        /// <summary>
        /// A dictionary of value types and a string parser (if they have one).
        /// </summary>
        private static readonly Dictionary<Type, ConvertDelegate> StringToValueDict = new()
        {
            { typeof(Boolean), (string value) => Boolean.TryParse(value, out var val) ? val : null },
            { typeof(Byte), (string value) => Byte.TryParse(value, out var val) ? val : null },
            { typeof(DateTime), (string value) => DateTime.TryParse(value, out var val) ? val : null },
            { typeof(Decimal), (string value) => Decimal.TryParse(value, out var val) ? val : null },
            { typeof(Double), (string value) => Double.TryParse(value, out var val) ? val : null },
            { typeof(Guid), (string value) => Guid.TryParse(value, out var val) ? val : null },
            { typeof(Int16), (string value) => Int16.TryParse(value, out var val) ? val : null },
            { typeof(Int32), (string value) => Int32.TryParse(value, out var val) ? val : null },
            { typeof(Int64), (string value) => Int64.TryParse(value, out var val) ? val : null },
            { typeof(SByte), (string value) => SByte.TryParse(value, out var val) ? val : null },
            { typeof(Single), (string value) => Single.TryParse(value, out var val) ? val : null },
            { typeof(UInt16), (string value) => UInt16.TryParse(value, out var val) ? val : null },
            { typeof(UInt32), (string value) => UInt32.TryParse(value, out var val) ? val : null },
            { typeof(UInt64), (string value) => UInt64.TryParse(value, out var val) ? val : null },
        };

        /// <summary>
        /// Delegate used in <see cref="ConverterHelper.ExcludedTypes"/>.
        /// </summary>
        private delegate object? GetDelegate(ref Utf8JsonReader reader);

        /// <summary>
        /// Delegate used in <see cref="ConverterHelper.StringToValueDict"/>.
        /// </summary>
        private delegate object? ConvertDelegate(string value);

        /// <summary>
        /// Returns an instance of data defined by a single value (ex: a string, an integer, a vector, a color, etc).
        /// </summary>
        /// <param name="reader">Utf8JsonReader used to provide the data for getting an object</param>
        /// <param name="type">The type of data. Decides what converter to use</param>
        /// <param name="settings">Serialization settings containing additional information/data needed for serialization
        /// </param>
        /// <param name="options">Options associated with the Utf8JsonReader</param>
        /// <returns>If there is a converter associated with the type parameter, then an object that has been passed
        /// through the associated converter is returned. If there is no such converter, then null is returned.
        /// If, for any reason, the conversion fails, null is also returned and a debug message is displayed
        /// </returns>
        public static object? GetValueFromReader(ref Utf8JsonReader reader, Type type, SerializationSettings settings,
            JsonSerializerOptions options)
        {
            //if the type already has an existing read method in Utf8JsonReader, then just use that method.
            if (ExcludedTypes.TryGetValue(type, out var getDelegate))
            {
                return getDelegate(ref reader);
            }

            var (converterType, converter) = settings.Converters.FirstOrDefault(c => c.Value.CanConvert(type));

            if (converter is null || converterType is null)
            {
                Debug.WriteLine($"Unable to find valid converter for type {type}. GetValueFromReader is returning " +
                                $"null.");
                return null;
            }

            //convertFromType is NOT the same as the type parameter. This is because some converters handle the
            //superclass /or parent of a type instead of the type driectly. An example would be EnumJsonConverter
            //handling the Enum type instead of specific enum types (which are all children of the Enum type).
            //convertFromType is the same as T in JsonConverter<T>
            Type convertFromType = converterType.BaseType?.GetGenericArguments()[0] ?? type;
            var readerHelperType = typeof(ReadHelper<>).MakeGenericType(convertFromType);
            if (Activator.CreateInstance(readerHelperType, converter) is not ReadHelper readHelper)
            {
                Debug.WriteLine($"Unable to create ReadHelper<{type}>. GetValueFromReader is returning null");
                return null;
            }

            return readHelper.Read(ref reader, type, options);
        }

        /// <summary>
        /// Returns an array of objects that represents the serialization of each element in a Json array.
        /// </summary>
        /// <param name="reader">Utf8JsonReader instance that provides the data to deserialize. The token type of the
        /// reader MUST initially be "TokenType.StartArray" or else the function will fail.</param>
        /// <param name="settings"><see cref="SerializationSettings"/> object whose
        /// <see cref="SerializationSettings.ExternalTypes"/> and <see cref="SerializationSettings.TypeAliases"/> are
        /// used to find <see cref="Type"/>s from type names.</param>
        /// <param name="options">Options associated with the Utf8JsonReader</param>
        /// <returns>If successful, returns an array of objects that represents the serialization of each element in a
        /// Json array. If unsuccessful, null is returned and a message is written to the debugger</returns>
        public static object?[]? GetArrayFromReader(ref Utf8JsonReader reader, SerializationSettings settings,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                Debug.WriteLine($"reader token type is initially not StartArray in GetArrayFromReader! Returning" +
                                $" null. (Reader token type: {reader.TokenType})");
                return null;
            }

            //valueType is going to be immutabletext or some other
            List<object?> outputList = new();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                Type? valueType = GetTypeFromObjectReader(ref reader, settings);
                if (valueType is null)
                {
                    Debug.WriteLine($"Type is null (GetArrayFromReader). Skipping object ...");
                    SkipObject(ref reader);
                    continue;
                }

                Type deserializeHelperType =
                    typeof(DeserializeHelper<>).MakeGenericType(Nullable.GetUnderlyingType(valueType) ?? valueType);

                if (Activator.CreateInstance(deserializeHelperType) is DeserializeHelper
                    deserializeHelperInstance)
                {
                    object? deserializedObject = deserializeHelperInstance.Deserialize(ref reader, settings, options);
                    
                    if (deserializedObject is not null)
                    {
                        outputList.Add(deserializedObject);
                    }
                    else
                    {
                        Debug.WriteLine($"Unable to deserialize object of type {valueType} in GetArrayFromReader");
                    }
                }
                
                while (reader.TokenType != JsonTokenType.EndArray && reader.TokenType != JsonTokenType.StartObject)
                {
                    reader.Read();
                }
            }

            return outputList.ToArray();
        }

        /// <summary>
        /// Returns a dictionary that represents the deserialization of a JSON object.
        /// </summary>
        /// <param name="reader">Utf8JsonReader instance that provides the data to deserialize. The token type of the
        /// reader MUST initially be "TokenType.StartObject" or else the function will fail.</param>
        /// <param name="settings"><see cref="SerializationSettings"/> object whose
        /// <see cref="SerializationSettings.ExternalTypes"/> and <see cref="SerializationSettings.TypeAliases"/> are
        /// used to find <see cref="Type"/>s from type names.</param>
        /// <param name="options">Options associated with the Utf8JsonReader</param>
        /// <returns>If successful, returns a dictionary that represents the deserialization of a JSON object. 
        /// If unsuccessful, null is returned and a message is written to the debugger.</returns>
        public static Dictionary<string, object>? GetDictionaryFromReader(ref Utf8JsonReader reader,
            SerializationSettings settings, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                Debug.WriteLine($"reader token type is initially not StartObject in GetDictionaryFromReader! " +
                                $"Returning null. (Reader token type: {reader.TokenType})");
                return null;
            }

            Dictionary<string, object> outputDict = new();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                while (reader.TokenType != JsonTokenType.PropertyName && reader.TokenType != JsonTokenType.EndObject)
                {
                    reader.Read();
                }
                
                if (reader.TokenType == JsonTokenType.EndObject) break;

                string key = reader.GetString()!; // cannot be null since TokenType is PropertyName at this point.
                
                Type? valueType = GetTypeFromObjectReader(ref reader, settings);
                if (valueType is null)
                {
                    Debug.WriteLine("Type is null (GetDictionaryFromReader). Skipping object ...");
                    SkipObject(ref reader);

                    continue;
                }

                Type deserializeHelperType =
                    typeof(DeserializeHelper<>).MakeGenericType(Nullable.GetUnderlyingType(valueType) ?? valueType);

                if (Activator.CreateInstance(deserializeHelperType) is not DeserializeHelper helper) continue;

                object? deserializedObject = helper.Deserialize(ref reader, settings, options);
                if (deserializedObject is null)
                {
                    Debug.WriteLine($"Value is null when trying to deserialize object of type {valueType} " +
                                    $"(GetDictionaryFromReader).");
                }
                else if (!outputDict.TryAdd(key, deserializedObject))
                {
                    Debug.WriteLine($"Key {key} in dictionary already exists. Skipping object ... " +
                                    $"(GetDictionaryFromReader).");
                }

                reader.Read();
            }

            return outputDict;
        }
        
        private static void SkipObject(ref Utf8JsonReader reader)
        {
            while (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.EndArray)
            {
                reader.Read();
            }

            reader.Read();
        }
        
        /// <summary>
        /// Gets the type string from a Utf8JsonReader.
        /// </summary>
        /// <param name="reader">Utf8JsonReader instance that provides the data.</param>
        /// <returns>The type string if found, or null if not found.</returns>
        private static string? GetTypeStringFromReader(ref Utf8JsonReader reader)
        {
            string typeIdentifier = SerializationSettings.TypeIdentifier;

            while ((reader.TokenType != JsonTokenType.PropertyName || reader.GetString()! != typeIdentifier) &&
                   reader.TokenType != JsonTokenType.EndObject)
            {
                reader.Read();
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                Debug.WriteLine($"Type specifier was not found in the object and the type could not be determined! " +
                                $"GetTypeStringFromReader (private) returning null.");

                return null;
            }

            reader.Read();

            return reader.GetString();
        }

        /// <summary>
        /// Finds the property with a name of <see cref="SerializationSettings.TypeIdentifier"/> in a Json object and
        /// returns the corresponding type. Returns null if not found.
        /// </summary>
        private static Type? GetTypeFromObjectReader(ref Utf8JsonReader reader, SerializationSettings settings)
        {
            string? typeStr = GetTypeStringFromReader(ref reader);
            if (typeStr is null)
            {
                return null;
            }

            return GetTypeFromString(typeStr, settings);
        }

        /// <summary>
        /// Tries to get a type associated with an object by trying to find a type through it's type identifier
        /// property.
        /// </summary>
        /// <param name="obj">The <see cref="JsonObject"/> to find the type of</param>
        /// <param name="settings"><see cref="SerializationSettings"/> object that is passed to
        /// <see cref="GetTypeFromString"/> to get a type from a type name.</param>
        /// <returns>If a type was found for the provided object, then a <see cref="Type"/> is returned. Otherwise,
        /// null is returned.</returns>
        public static Type? GetTypeFromObject(JsonObject obj, SerializationSettings settings)
        {
            foreach (JsonProperty prop in obj.Properties)
            {
                if (prop.Name == SerializationSettings.TypeIdentifier && prop.Value is string typeName)
                {
                    return GetTypeFromString(typeName, settings);
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to obtain a <see cref="Type"/> from name using <see cref="SerializationSettings"/>
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="settings">Provides <see cref="SerializationSettings.TypeAliases"/> and
        /// <see cref="SerializationSettings.ExternalTypes"/> to get a type from a type name.</param>
        /// <returns>If <see cref="typeName"/> is a valid type name or a valid alias for a type, then the type that
        /// <see cref="typeName"/> represents is returned. Otherwise, null is returned.</returns>
        public static Type? GetTypeFromString(string typeName, SerializationSettings settings)
        {
            if (settings.TypeAliases.TryGetValue(typeName, out var alias))
            {
                typeName = alias;
            }

            Type? valueType = Type.GetType(typeName!);
            if (valueType is not null) return valueType;

            if (settings.ExternalTypes.TryGetValue(typeName, out valueType))
            {
                return valueType;
            }

            Debug.WriteLine(
                $"{nameof(ConverterHelper)}.{nameof(GetTypeFromString)}: Can't find type of name {typeName}! " +
                "Ensure that the type exists in SerializationSettings.ExternalTypes and that the type name is correct.");

            return null;
        }

        /// <summary>
        /// Returns data that is defined by using object syntax (ex: Borders)
        /// </summary>
        /// <param name="reader">Utf8JsonReader instance that provides the data to deserialize. It is preferable that
        /// the TokenType property first be "JsonTokenType.StartObject"</param>
        /// <param name="type">The type of the object to deserialize</param>
        /// <param name="settings"><see cref="SerializationSettings"/> object whose
        /// <see cref="SerializationSettings.ExternalTypes"/> and <see cref="SerializationSettings.TypeAliases"/> are
        /// used to find <see cref="Type"/>s from type names.</param>
        /// <param name="options">Options associated with the Utf8JsonReader</param>
        /// <returns>Returns an object that is representative of the Json data provided by the reader. If null, then
        /// the deserialization was unsuccessful</returns>
        public static object? GetObjectFromReader(ref Utf8JsonReader reader, Type type, SerializationSettings settings,
            JsonSerializerOptions options)
        {
            //If the given type does not (or cannot) have a valid JsonConstructor, then see if the object we are going
            //to deserialize has one.
            if (!type.IsTypeJsonSerializable())
            {
                type = GetTypeFromObjectReader(ref reader, settings) ?? type;
            }

            Type deserializeHelperType =
                typeof(DeserializeHelper<>).MakeGenericType(Nullable.GetUnderlyingType(type) ?? type);

            if (Activator.CreateInstance(deserializeHelperType) is DeserializeHelper deserializeHelperInstance)
            {
                return deserializeHelperInstance.Deserialize(ref reader, settings, options);
            }

            return null;
        }

        /// <summary>
        /// Determines if a <see cref="Type"/> is able to be Json serialized.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>If a type is neither an object, nor an interface, nor an abstract class, then it is Json
        /// serializable and therefore this method returns true. Otherwise, false</returns>
        public static bool IsTypeJsonSerializable(this Type type) =>
            !(type == typeof(object) || type.IsInterface || type.IsAbstract);
        
        //we can't pass values by reference using reflection, so we use this hacky solution to do so
        //(https://stackoverflow.com/questions/60830084/how-to-pass-an-argument-by-reference-using-reflection)

        /// <summary>
        /// Abstract class that allows the calling of Deserialize with a reference to a UTf8JsonReader as a parameter
        /// within a reflection context with a delegate hack. Based off of
        /// (https://stackoverflow.com/questions/60830084/how-to-pass-an-argument-by-reference-using-reflection)
        /// </summary>
        public abstract class ReadHelper
        {
            public abstract object? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options);
        }

        private class ReadHelper<T> : ReadHelper
        {
            private delegate T? ReadDelegate(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options);

            private readonly ReadDelegate? _readDelegate;

            public ReadHelper(JsonConverter<T> converter)
            {
                _readDelegate = converter.Read;
            }

            public override object? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            {
                if (_readDelegate is null) return null;

                return _readDelegate.Invoke(ref reader, type, options);
            }
        }

        /// <summary>
        /// Abstract class that allows the calling of Deserialize with a reference to a UTf8JsonReader as a parameter
        /// within a reflection context with a delegate hack. Based off of
        /// https://stackoverflow.com/questions/60830084/how-to-pass-an-argument-by-reference-using-reflection
        /// </summary>
        public abstract class DeserializeHelper
        {
            public abstract object? Deserialize(ref Utf8JsonReader reader, SerializationSettings settings,
                JsonSerializerOptions options);
        }

        /// <summary>
        /// Allows the DeserializeHelper abstract class to call Deserialize with a specified generic/return type
        /// </summary>
        /// <typeparam name="T">The return type of the Deserialize method</typeparam>
        public class DeserializeHelper<T> : DeserializeHelper
        {
            private delegate T? DeserializeDelegate(ref Utf8JsonReader reader, SerializationSettings settings,
                JsonSerializerOptions options);

            private readonly DeserializeDelegate? _deserializeDelegate;

            public DeserializeHelper()
            {
                _deserializeDelegate = Serializer.Deserialize<T>;
            }

            public override object? Deserialize(ref Utf8JsonReader reader, SerializationSettings settings,
                JsonSerializerOptions options)
            {
                if (_deserializeDelegate is null) return null;
                
                object? deserializedObject = _deserializeDelegate.Invoke(ref reader, settings, options);

                return deserializedObject switch
                {
                    ValueInfo valueInfo => valueInfo.TryGetValue(out object? value) ? value : null,
                    _ => deserializedObject
                };
            }
        }
    }
}