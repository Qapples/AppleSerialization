namespace AppleSerialization.Json
{
    /// <summary>
    /// Represents a json element that is capable of having a name.
    /// </summary>
    public interface IName
    {
        /// <summary>
        /// Name of the element.
        /// </summary>
        public string? Name { get; set; }
    }
}