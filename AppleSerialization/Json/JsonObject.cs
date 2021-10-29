using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DefaultEcs;
using Microsoft.Xna.Framework;

namespace AppleSerialization.Json
{
    /// <summary>
    /// Represents a collection of <see cref="JsonProperty"/> instance that describe each individual property and value
    /// of an object represented in json. Also contains other <see cref="JsonObject"/> instances as children
    /// </summary>
    public class JsonObject : IName, ICloneable
    {
        /// <summary>
        /// The name of this object. If null, then the object in question does not have a name (i.e. an element in an
        /// array)
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// The parent of this object. If null, then this object does not have a parent.
        /// </summary>
        /// <remarks>If the object is an element of a <see cref="JsonArray"/>, the parent will be the parent of that
        /// <see cref="JsonArray"/>.</remarks>
        public JsonObject? Parent { get; set; }
    
        /// <summary>
        /// <see cref="JsonProperty"/> instances that describe the properties (and their values) of this object.
        /// </summary>
        public IList<JsonProperty> Properties { get; set; }
        
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
        /// <param name="name">If available, this represents the name of the object. Not every object will and can have
        /// a name (i.e. elements in an array). If this value is not set to, then a name or identifier will attempt to
        /// be found by finding a value in <see cref="Properties"/> whose name is "id" or "name". If not found, then
        /// <see cref="Name"/> will be null.</param>
        /// <param name="parent">The parent of this object. If null, then this object does not have a parent.</param>
        /// <param name="properties">see cref="JsonProperty"/> instances that describe the properties (and their values)
        /// of the object. If this parameter null, a new <see cref="List{T}"/> will be created.</param>
        /// <param name="children"><see cref="JsonObject"/> instances that are within the instance. If this parameter
        /// is null, a new <see cref="List{T}"/> will be created.</param>
        /// <param name="arrays"><see cref="JsonArray"/> instance that represent any and all arrays the object will
        /// have. If null, a new <see cref="List{T}"/> will be created.</param>
        public JsonObject(string? name = null, JsonObject? parent = null, IList<JsonProperty>? properties = null,
            IList<JsonObject>? children = null, IList<JsonArray>? arrays = null)
        {
            (Name, Parent, Properties, Children, Arrays) = (name, parent, properties ?? new List<JsonProperty>(),
                children ?? new List<JsonObject>(), arrays ?? new List<JsonArray>());
        }
        
        
        /// <summary>
        /// Creates a new <see cref="JsonObject"/> instance based on the data received from a
        /// <see cref="Utf8JsonReader"/> instance.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> instance that provides the data necessary to create
        /// a new <see cref="JsonObject"/> instance. In most cases, the <see cref="Utf8JsonReader"/> is receiving
        /// data from a file.</param>
        /// <param name="parent">The parent of the newly created instance. If null, then the object has no parent. By
        /// default null.</param>
        /// <returns>A new <see cref="JsonObject"/> object that represents the json data given to by the
        /// <see cref="Utf8JsonReader"/>. If either of the following values are null in <see cref="Environment"/>: <br/>
        /// <see cref="Environment.DefaultFontSystem"/> <br/>
        /// <see cref="Environment.GraphicsDevice"/> <br/>
        /// <see cref="Environment.ContentManager"/> <br/>
        /// OR if there was a parsing error then null is returned along with a debug message.</returns>
        public static JsonObject? CreateFromJsonReader(ref Utf8JsonReader reader, JsonObject? parent = null)
        {
            const string methodName = nameof(JsonObject) + "." + nameof(CreateFromJsonReader);
            
            //Ensure that the proper variables were set to in Environment
            if (Environment.DefaultFontSystem is null || Environment.GraphicsDevice is null ||
                Environment.ContentManager is null)
            {
                Debug.WriteLine($"{methodName}: One or more of the environment variables are null: " +
                                $"\nDefaultFontSystem: {(Environment.DefaultFontSystem is null ? "null" : "not null")} " +
                                $"\nGraphicsDevice: {(Environment.GraphicsDevice is null ? "null" : "not null")} " +
                                $"\nContentManager: {(Environment.ContentManager is null ? "null" : "not null")}" +
                                "\n null returned.");
        
                return null;
            }

            JsonObject rootObject = new() {Parent = parent};

            try
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName) continue;

                    string propertyName = reader.GetString()!; //PropertyNames will always be a string
                    if (!reader.Read()) break; //skip to next node

                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        //TODO: If jsonArray is null, then undefined behavior could come up. Be careful!
                        JsonArray? jsonArray = GetObjectArray(ref reader, propertyName, rootObject);
                        if (jsonArray is null) break;

                        rootObject.Arrays.Add(jsonArray);
                    }
                    else if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        JsonObject? child = CreateFromJsonReader(ref reader, rootObject);
                        
                        if (child is not null)
                        {
                            child.Name = propertyName;
                            rootObject.Children.Add(child);
                        }
                    }
                    else
                    {
                        string lowerPropertyName = propertyName.ToLower();

                        object? parameterValue = null;
                        JsonValueKind valueKind = JsonValueKind.Null;
                        switch (reader.TokenType)
                        {
                            // ReSharper disable MultipleStatementsOnOneLine
                            case JsonTokenType.True:
                                parameterValue = true;
                                valueKind = JsonValueKind.True;
                                break;
                            case JsonTokenType.False:
                                parameterValue = false;
                                valueKind = JsonValueKind.False;
                                break;
                            case JsonTokenType.String:
                                parameterValue = reader.GetString();
                                valueKind = JsonValueKind.String;
                                break;
                            case JsonTokenType.Number:
                                parameterValue = GetNumber(ref reader);
                                valueKind = JsonValueKind.Number;
                                break;
                            // ReSharper enable MultipleStatementsOnOneLine
                        }

                        if (lowerPropertyName is "size" or "scale" && parameterValue is not null)
                        {
                            Environment.CurrentDeserializingObjectSize = (Vector2) parameterValue;
                        }

                        rootObject.Properties.Add(new JsonProperty(propertyName, parameterValue, rootObject,
                            valueKind));
                    }
                }
            }
            catch (JsonException e)
            {
                Debug.WriteLine($"{methodName}: error parsing json with exception and returning null: {e}");
                return null;
            }
            
            //sometimes objects will have an "id" or "name" property. if the object doesn't already have a name, find
            //that property so we can give it a name.
            if (rootObject.Name is null && rootObject.Properties.Count > 0)
            {
                rootObject.Name = rootObject.FindName();
            }
            
            return rootObject;
        }

        private string? FindName() =>
            Properties.FirstOrDefault(p => p.Name?.ToLower() is "id" or "name")?.Value as string;
        
        private static readonly JsonWriterOptions DefaultWriterOptions = new() {Indented = true};

        /// <summary>
        /// Generates a string that is representative of this <see cref="JsonObject"/> and it's children, properties,
        /// and arrays in json format.
        /// </summary>
        /// <param name="options">Optional <see cref="JsonWriterOptions"/> instance that determines how the string is
        /// generated.</param>
        /// <returns>A string value that represents this <see cref="JsonObject"/> and it's children, properties, and
        /// arrays in json format.</returns>
        public string GenerateJsonText(in JsonWriterOptions? options = null)
        {
            using MemoryStream ms = new();

            Utf8JsonWriter writer = new(ms, options ?? DefaultWriterOptions);
            WriteToJson(writer);
            writer.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        /// <summary>
        /// Attempts to write the contents of this <see cref="JsonObject"/> instance to a file path.
        /// </summary>
        /// <param name="filePath">The path of the file to write to.</param>
        /// <param name="options">Optional <see cref="JsonWriterOptions"/> instance that determines how the file is
        /// written to. If null, then <see cref="DefaultWriterOptions"/> is used.</param>
        /// <returns>If writing to the file was successful, then true is returned. Otherwise, false is returned and a
        /// message is written to the debugger explaining what went wrong.</returns>
        public bool TryWriteToFile(string filePath, in JsonWriterOptions? options = null)
        {
            try
            {
                using FileStream stream = File.OpenWrite(filePath);

                Utf8JsonWriter writer = new(stream, options ?? DefaultWriterOptions);
                WriteToJson(writer);
                writer.Flush();

                return true;
            }
            catch (Exception e)
            { 
                Debug.WriteLine($"{nameof(TryWriteToFile)} failed with exception: {e}.");
                return false;
            }
        }
        
        //--------------
        // Find methods
        //--------------
        //we could use a generic here to reduce code reuse here, but I figured it would be much more straightforward
        //to have three different methods to simplify use here. 
        
        /// <summary>
        /// Recursively finds a <see cref="JsonObject"/> instance within <see cref="Children"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="JsonObject"/> instance to find.</param>
        /// <param name="comparison">Optional <see cref="StringComparison"/> enum that determines how names should be
        /// compared. By default, it is <see cref="StringComparison.Ordinal"/></param>
        /// <returns>If found, then the <see cref="JsonObject"/> with the specified name is returned. Otherwise, null
        /// is returned.</returns>
        public JsonObject? FindChild(in string name, in StringComparison comparison = StringComparison.Ordinal)
        {
            foreach (JsonObject child in Children)
            {
                if (string.Equals(name, child.Name, comparison)) return child;

                JsonObject? returnObj = child.FindChild(in name, in comparison);
                if (returnObj is not null) return returnObj;
            }

            return null;
        }

        /// <summary>
        /// Recursively finds a <see cref="JsonProperty"/> instance within <see cref="Properties"/> in this
        /// instance and it's <see cref="Children"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="JsonProperty"/> instance to find.</param>
        /// <param name="comparison">Optional <see cref="StringComparison"/> enum that determines how names should be
        /// compared. By default, it is <see cref="StringComparison.Ordinal"/></param>
        /// <returns>If found, then the <see cref="JsonProperty"/> with the specified name is returned. Otherwise, null
        /// is returned.</returns>
        public JsonProperty? FindProperty(in string name, in StringComparison comparison = StringComparison.Ordinal)
        {
            foreach (JsonProperty property in Properties)
            {
                if (string.Equals(name, property.Name, comparison)) return property;
            }

            foreach (JsonObject child in Children)
            {
                JsonProperty? returnProp = child.FindProperty(in name, in comparison);
                if (returnProp is not null) return returnProp;
            }

            return null;
        }
        
        /// <summary>
        /// Recursively finds a <see cref="JsonArray"/> instance within <see cref="Arrays"/> in this
        /// instance and it's <see cref="Children"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="JsonArray"/> instance to find.</param>
        /// <param name="comparison">Optional <see cref="StringComparison"/> enum that determines how names should be
        /// compared. By default, it is <see cref="StringComparison.Ordinal"/></param>
        /// <returns>If found, then the <see cref="JsonArray"/> with the specified name is returned. Otherwise, null
        /// is returned.</returns>
        public JsonArray? FindArray(in string name, in StringComparison comparison = StringComparison.Ordinal)
        {
            foreach (JsonArray array in Arrays)
            {
                if (string.Equals(name, array.Name, comparison)) return array;
            }

            foreach (JsonObject child in Children)
            {
                JsonArray? returnArr = child.FindArray(in name, in comparison);
                if (returnArr is not null) return returnArr;
            }

            return null;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="JsonObject"/> instance including it's <see cref="Properties"/>,
        /// <see cref="Children"/>, and <see cref="Arrays"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="JsonObject"/> whose data is identical but separate from this instance.
        /// </returns>
        /// <remarks>The <see cref="Parent"/> of the new instance will be the same as this one.</remarks>
        public object Clone() => new JsonObject(Name, Parent, Properties.MemberClone(), Children.MemberClone(),
            Arrays.MemberClone());

        /// <summary>
        /// Combines the properties, arrays, and children of two instances of <see cref="JsonObject"/> and creates a
        /// new <see cref="JsonObject"/> instance from it.
        /// </summary>
        /// <param name="a">The first <see cref="JsonObject"/> instance.</param>
        /// <param name="b">The second <see cref="JsonObject"/> instance.</param>
        /// <returns>A new <see cref="JsonObject"/> instance whose properties, arrays, and children from a and b are
        /// combined.</returns>
        public static JsonObject operator +(JsonObject a, JsonObject b)
        {
            JsonObject outObject = new();

            foreach (JsonProperty property in a.Properties) outObject.Properties.Add(property);
            foreach (JsonProperty property in b.Properties) outObject.Properties.Add(property);
            
            foreach (JsonArray array in a.Arrays) outObject.Arrays.Add(array);
            foreach (JsonArray array in b.Arrays) outObject.Arrays.Add(array);
            
            foreach (JsonObject child in a.Children) outObject.Children.Add(child);
            foreach (JsonObject child in b.Children) outObject.Children.Add(child);

            return outObject;
        }

        private void WriteToJson(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            
            //arrays
            foreach (JsonArray arr in Arrays)
            {
                if (arr.Name is not null) writer.WritePropertyName(arr.Name);
                writer.WriteStartArray();
                
                foreach (JsonObject arrObj in arr.Objects)
                {
                    //Debug.WriteLine(arrObj.Properties.First().Value);
                    if (arrObj.Name is not null) writer.WritePropertyName(arrObj.Name);
                    arrObj.WriteToJson(writer);
                }
                
                writer.WriteEndArray();
            }
            
            //properties
            foreach (JsonProperty prop in Properties)
            {
                WriteProperty(writer, prop);
            }
            
            //children
            foreach (JsonObject child in Children)
            {
                if (child.Name is not null) writer.WritePropertyName(child.Name);
                child.WriteToJson(writer);
            }

            writer.WriteEndObject();
        }
        
        private static JsonArray? GetObjectArray(ref Utf8JsonReader reader, string name, JsonObject? parent = null)
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
                    JsonObject? newObject = CreateFromJsonReader(ref reader, parent);

                    if (newObject is not null && (newObject.Children.Count > 0 || newObject.Properties.Count > 0))
                    {
                        outputList.Add(newObject);
                    }

                    reader.Read();
                }
            }

            return new JsonArray(name, parent, outputList);
        }
        
        private static void WriteProperty(Utf8JsonWriter writer, JsonProperty property)
        {
            const string methodName = nameof(JsonObject) + "." + nameof(WriteProperty);

            if (property.Value is null)
            {
                Debug.WriteLine($"{methodName}: property does not have a value. Cannot write!");
                return;
            }
            
            if (property.Name is not null) writer.WritePropertyName(property.Name);
            
            switch (property.ValueKind)
            {
                case JsonValueKind.True: writer.WriteBooleanValue(true); break;
                case JsonValueKind.False: writer.WriteBooleanValue(false); break;
                case JsonValueKind.Null: writer.WriteNullValue(); break;
                case JsonValueKind.String: writer.WriteStringValue((string) property.Value); break;
                case JsonValueKind.Number: WriteNumber(writer, property.Value);break;
            }
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
        
        // ReSharper disable BuiltInTypeReferenceStyle
        private static void WriteNumber(Utf8JsonWriter writer, object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                //Decimal/Point values
                case TypeCode.Single: writer.WriteNumberValue((Single) value); break;
                case TypeCode.Double: writer.WriteNumberValue((Double) value); break;
                case TypeCode.Decimal: writer.WriteNumberValue((Decimal) value); break;

                //int values. these value all result in the "int" overload of the WriteNumberValue to be used so that's
                //why they're all under here.
                case TypeCode.Byte: case TypeCode.SByte: case TypeCode.Int16: case TypeCode.UInt16: case TypeCode.Int32:
                    writer.WriteNumberValue((int) value); break;
                
                //larger int values
                case TypeCode.UInt32: writer.WriteNumberValue((UInt32) value); break;
                case TypeCode.UInt64: writer.WriteNumberValue((UInt64) value); break;
                case TypeCode.Int64: writer.WriteNumberValue((Int64) value); break;
            }
        }

        //this is an array because we don't actually know the exact type of the number before hand. therefore, we have
        //to go through this array and brute force it (there aren't that many types so for the most part it should be
        //fine)
        private static readonly TryGetDelegate[] GetNumberDelegates =
        {
            // ReSharper disable BuiltInTypeReferenceStyle
            //we don't want to use in the built in type references here to match up with the reader methods

            (ref Utf8JsonReader reader) => reader.TryGetInt32(out var value) ? value : null,

            //decimal values
            (ref Utf8JsonReader reader) => reader.TryGetSingle(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetDouble(out var value) ? value : null,
            (ref Utf8JsonReader reader) => reader.TryGetDecimal(out var value) ? value : null,
        };

        private delegate object? TryGetDelegate(ref Utf8JsonReader reader);
    }
}