using System;
using FastDeepCloner;
using JsonValueKind = System.Text.Json.JsonValueKind;

namespace AppleSerialization.Json
{
    /// <summary>
    /// Represents the data of a Json property.
    /// </summary>
    public class JsonProperty : IName, ICloneable
    {
        /// <summary>
        /// The name of the property. If null, then this property does not have a name and is likely an element of
        /// an array.
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// The value of the property. If null, then this property does not have a parent.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// The parent of this property.
        /// </summary>
        /// <remarks>If the property is an element of a <see cref="JsonArray"/>, the parent will be the parent of that
        /// <see cref="JsonArray"/>.</remarks>
        public JsonObject? Parent { get; set; }

        /// <summary>
        /// Represents the the type of value that this property represents. 
        /// </summary>
        public JsonValueKind ValueKind { get; set; }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonProperty"/>
        /// </summary>
        /// <param name="name">The name of the property. If not set to, the default value is null</param>
        /// <param name="value">The value of the property. If not set to, the default value is null.</param>
        /// <param name="parent">The parent of this property. If null, then this object does not have a parent.
        /// If not set to, the default value is null.</param>
        /// <param name="valueKind">Represents the the type of value that this object represents. If not set to, the
        /// default value is <see cref="JsonValueKind.Null"/>.</param>
        public JsonProperty(string? name = null, object? value = null, JsonObject? parent = null,
            JsonValueKind valueKind = JsonValueKind.Null) =>
            (Name, Value, Parent, ValueKind) = (name, value, parent, valueKind);

        /// <summary>
        /// Default constructor that creates a blank <see cref="JsonProperty"/> instance via calling
        /// <see cref="JsonProperty(string?, object?, JsonObject?, JsonValueKind)"/>
        /// </summary>
        public JsonProperty() : this(null, null, null, JsonValueKind.Null)
        {
            //We have a default constructor here in the case that we want to call the other constructor with all null
            //values via reflection more simply and easily.
        }

        /// <summary>
        /// Creates a deep clone of this <see cref="JsonProperty"/> instance along with a deep clone of
        /// <see cref="Value"/>. If <see cref="Value"/> does not implement <see cref="ICloneable"/> then
        /// <see cref="DeepCloner"/> will be used instead to clone the object.
        /// </summary>
        /// <returns>A new deep clone instance that has identical data to this <see cref="JsonProperty"/> instance.
        /// </returns>
        /// <remarks>The <see cref="Parent"/> of the new instance will be the same as this one.</remarks>
        public object Clone()
        {
            if (Value is ICloneable cloneable)
            {
                return new JsonProperty(Name, Value is null ? null : cloneable.Clone(), Parent, ValueKind);
            }

            object valueClone = Value.Clone();
            return new JsonProperty(Name, Value is null ? null : valueClone, Parent, ValueKind);
        }
    }
}