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
    /// Represents a collection of <see cref="JsonProperty"/> instance that describe each individual property and value
    /// of an object represented in json. Also contains other <see cref="JsonObject"/> instances as children
    /// </summary>
    public class JsonObject
    {
        /// <summary>
        /// <see cref="JsonProperty"/> instances that describe the properties (and their values) of this object.
        /// </summary>
        public IList<JsonProperty> Elements { get; set; }
        
        /// <summary>
        /// <see cref="JsonObject"/> instances that are within this instance.
        /// </summary>
        public IList<JsonObject> Children { get; set; }

        /// <summary>
        /// <see cref="JsonArray"/> instance that represent any and all arrays this object has.
        /// </summary>
        public IList<JsonArray> Arrays { get; set; }

        /// <summary>
        /// Constructs a new <see cref="JsonObject"/> instance.
        /// </summary>
        /// <param name="elements">see cref="JsonProperty"/> instances that describe the properties (and their values)
        /// of the object. If this parameter null, a new <see cref="List{T}"/> will be created.</param>
        /// <param name="children"><see cref="JsonObject"/> instances that are within the instance. If this parameter
        /// is null, a new <see cref="List{T}"/> will be created.</param>
        /// <param name="arrays"><see cref="JsonArray"/> instance that represent any and all arrays the object will
        /// have. If null, a new <see cref="List{T}"/> will be created.</param>
        public JsonObject(IList<JsonProperty>? elements = null, IList<JsonObject>? children = null,
            IList<JsonArray>? arrays = null) => (Elements, Children, Arrays) =
            (elements ?? new List<JsonProperty>(), children ?? new List<JsonObject>(), arrays ?? new List<JsonArray>());

        /// <summary>
        /// Constructs a new <see cref="JsonObject"/> instance based on the data received from a
        /// <see cref="Utf8JsonReader"/> instance.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> instance that provides the data necessary to create
        /// a new <see cref="JsonObject"/> instance. In most cases, the <see cref="Utf8JsonReader"/> is reciving
        /// data from a file.</param>
        public JsonObject(ref Utf8JsonReader reader)
        {
            JsonObject? jsonObject = CreateFromJsonReader(ref reader);

            (Elements, Children, Arrays) = (jsonObject?.Elements ?? new List<JsonProperty>(),
                jsonObject?.Children ?? new List<JsonObject>(), jsonObject?.Arrays ?? new List<JsonArray>());
        }

        /// <summary>
        /// Creates a new <see cref="JsonObject"/> instance based on the data received from a
        /// <see cref="Utf8JsonReader"/> instance.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> instance that provides the data necessary to create
        /// a new <see cref="JsonObject"/> instance. In most cases, the <see cref="Utf8JsonReader"/> is reciving
        /// data from a file.</param>
        /// <returns>A new <see cref="JsonObject"/> object that represents the json data given to by the
        /// <see cref="Utf8JsonReader"/>.</returns>
        public static JsonObject? CreateFromJsonReader(ref Utf8JsonReader reader)
        {
            //Ensure that the proper variables were set to in Environment
            if (Environment.DefaultFontSystem is null || Environment.GraphicsDevice is null ||
                Environment.ContentManager is null)
            {
                Debug.WriteLine("One or more of the enviornment variables are null: " +
                                $"\nDefaultFontSystem: {(Environment.DefaultFontSystem is null ? "null" : "not null")} " +
                                $"\nGraphicsDevice: {(Environment.GraphicsDevice is null ? "null" : "not null")} " +
                                $"\nContentManager: {(Environment.ContentManager is null ? "null" : "not null")}");
        
                return null;
            }

            JsonObject rootObject = new();
            
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) continue;
        
                string propertyName = reader.GetString()!; //PropertyNames will always be a string
                if (!reader.Read()) break; //skip to next node
        
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    //TODO: If elements is null, then undefined behavior could come up. Be careful!
                    JsonArray? elements = GetObjectArray(ref reader, propertyName);
                    if (elements is null) break;

                    rootObject.Arrays.Add(elements);
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    rootObject.Children.Add(new JsonObject(ref reader));
                }
                else
                {
                    string lowerPropertyName = propertyName.ToLower();

                    object? parameterValue = null;
                    switch (reader.TokenType)
                    {
                        // ReSharper disable MultipleStatementsOnOneLine
                        case JsonTokenType.True: parameterValue = true; break;
                        case JsonTokenType.False: parameterValue = false; break;
                        case JsonTokenType.String: parameterValue = reader.GetString(); break;
                        case JsonTokenType.Number: parameterValue = GetNumber(ref reader); break;
                        // ReSharper enable MultipleStatementsOnOneLine
                    }

                    if (lowerPropertyName is "size" or "scale" && parameterValue is not null)
                    {
                        Environment.CurrentDeserializingObjectSize = (Vector2) parameterValue;
                    }
        
                    rootObject.Elements.Add(new JsonProperty(propertyName, parameterValue));
                }
            }
        
            return rootObject;
        }
        
        private static JsonArray? GetObjectArray(ref Utf8JsonReader reader, string name)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                Debug.WriteLine($"reader token type is initially not StartArray in GetObjectArray! Returning" +
                                $" null.. (Reader token type: {reader.TokenType})");
                return null;
            }

            List<JsonObject> outputList = new();

            while (reader.TokenType != JsonTokenType.EndArray && reader.Read())
            {
                while (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.EndArray)
                {
                    JsonObject newObject = new(ref reader);

                    if (newObject.Children.Count > 0 || newObject.Elements.Count > 0)
                    {
                        outputList.Add(newObject);
                    }

                    reader.Read();
                }
            }

            return new JsonArray(name, outputList);
        }

        private static object? GetNumber(ref Utf8JsonReader reader)
        {
            foreach (var getDelegate in GetNumberDelegates)
            {
                object? value = getDelegate(ref reader);

                if (getDelegate(ref reader) is not null) return value;
            }

            return null;
        }

        private static readonly TryGetDelegate[] GetNumberDelegates =
        {
            // ReSharper disable BuiltInTypeReferenceStyle
            //we don't want to use in the built in type references here to match up with the reader methods

            //decimal values
            (ref Utf8JsonReader reader) => reader.TryGetDecimal(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetDouble(out var value) ? value : null,

            //byte values
            (ref Utf8JsonReader reader) => reader.TryGetByte(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetSByte(out var value) ? value : null,

            //int values
            (ref Utf8JsonReader reader) => reader.TryGetUInt16(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetInt16(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetUInt32(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetInt32(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetUInt64(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetInt64(out var value) ? value : null,
        };

        private delegate object? TryGetDelegate(ref Utf8JsonReader reader);
    }

    /// <summary>
    /// Represents the data of a json property.
    /// </summary>
    /// <param name="Name">The name of the property.</param>
    /// <param name="Value">The value of the property.</param>
    public sealed record JsonProperty(string Name, object? Value);

    /// <summary>
    /// Represents an array of <see cref="JsonObject"/> instances.
    /// </summary>
    /// <param name="Name">Name/Identifier of the array.</param>
    /// <param name="Objects">The <see cref="JsonObject"/> instances in the array.</param>
    public sealed record JsonArray(string Name, IList<JsonObject> Objects);
}