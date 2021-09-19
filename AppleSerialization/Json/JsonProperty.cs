using System.Text.Json;

namespace AppleSerialization.Json
{
    /// <summary>
    /// Represents the data of a Json property.
    /// </summary>
    public class JsonProperty : IName
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// The value of the property. If null, then this object does not have a parent.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// The parent of this property.
        /// </summary>
        /// <remarks>If the property is an element of a <see cref="JsonArray"/>, the parent will be the parent of that
        /// <see cref="JsonArray"/>.</remarks>
        public JsonObject? Parent { get; set; }

        /// <summary>
        /// Represents the the type of value that this object represents. 
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
            in JsonValueKind valueKind = JsonValueKind.Null) =>
            (Name, Value, Parent, ValueKind) = (name, value, parent, valueKind);
    }
}