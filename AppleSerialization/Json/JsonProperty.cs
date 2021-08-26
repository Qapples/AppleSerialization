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
        public string Name { get; set; }

        /// <summary>
        /// The value of the property.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Represents the the type of value that this object represents. 
        /// </summary>
        public JsonValueKind ValueKind { get; set; }

        /// <summary>
        /// Constructs a new instance of <see cref="JsonProperty"/>
        /// </summary>
        /// <param name="name">The name of the property. If not set to, the default value is a blank string ("")</param>
        /// <param name="value">The value of the property. If not set to, the default value is null.</param>
        /// <param name="valueKind">Represents the the type of value that this object represents. If not set to, the
        /// default value is <see cref="JsonValueKind.Null"/>.</param>
        public JsonProperty(string name = "", object? value = null, in JsonValueKind valueKind = JsonValueKind.Null) =>
            (Name, Value, ValueKind) = (name, value, valueKind);
    }
}