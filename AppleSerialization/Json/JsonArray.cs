using System.Collections;
using System.Collections.Generic;

namespace AppleSerialization.Json
{
    /// <summary>
    /// Represents an array of <see cref="JsonObject"/> instances.
    /// </summary>
    public class JsonArray : ICollection<JsonObject>
    {
        /// <summary>
        /// Name/Identifier of the array. Not all instances will have one.
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// The <see cref="JsonObject"/> instances in the array.
        /// </summary>
        public IList<JsonObject> Objects { get; set; }

        /// <summary>
        /// Constructs a <see cref="JsonArray"/> instance.
        /// </summary>
        /// <param name="name">Name/Identifier of the array. Not all instances will have one.</param>
        /// <param name="objects">The <see cref="JsonObject"/> instances in the array.</param>
        public JsonArray(string name, IList<JsonObject> objects) => (Name, Objects) = (name, objects);

        public IEnumerator<JsonObject> GetEnumerator() => Objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Objects.GetEnumerator();

        public void Add(JsonObject item) => Objects.Add(item);

        public void Clear() => Objects.Clear();

        public bool Contains(JsonObject item) => Objects.Contains(item);

        public void CopyTo(JsonObject[] array, int arrayIndex) => Objects.CopyTo(array, arrayIndex);

        public bool Remove(JsonObject item) => Objects.Remove(item);

        public int Count => Objects.Count;
        
        public bool IsReadOnly => Objects.IsReadOnly;
    }
}