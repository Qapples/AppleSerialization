using System.Text.Json;

namespace AppleSerialization.Json
{
    /// <summary>
    /// Represents the data of a json property.
    /// </summary>
    /// <param name="Name">The name of the property.</param>
    /// <param name="Value">The value of the property.</param>
    /// <param name="ValueKind">The <see cref="JsonValueKind"/> enum that determines what type of value the property
    /// represents. </param>
    public sealed record JsonProperty(string Name, object? Value, in JsonValueKind ValueKind)
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; set; } = Name;

        /// <summary>
        /// The value of the property.
        /// </summary>
        public object? Value { get; set; } = Value;

        /// <summary>
        /// Represents the the type of value that this object represents. 
        /// </summary>
        public JsonValueKind ValueKind { get; set; } = ValueKind;
    }
}