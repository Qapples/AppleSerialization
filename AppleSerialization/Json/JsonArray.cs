using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppleSerialization.Json
{
    /// <summary>
    /// Represents an array of <see cref="JsonObject"/> instances.
    /// </summary>
    public class JsonArray : ICollection<JsonObject>, IName, ICloneable
    {
        /// <summary>
        /// Name/Identifier of the array. Not all instances will have one.
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// The parent of this object.
        /// </summary>
        /// <remarks>
        /// If the array is an element of another <see cref="JsonArray"/>, the parent will be the parent of that
        /// <see cref="JsonArray"/>.</remarks>
        public JsonObject? Parent { get; set; }
        
        /// <summary>
        /// The <see cref="JsonObject"/> instances in the array.
        /// </summary>
        public IList<JsonObject> Objects { get; set; }

        /// <summary>
        /// Constructs a <see cref="JsonArray"/> instance.
        /// </summary>
        /// <param name="name">Name/Identifier of the array. Not all instances will have one. If the instance does not
        /// have a name or identifier, then the <see cref="Name"/> property will be null.</param>
        /// <param name="parent">The parent of this object. If null, then this object does not have a parent.</param>
        /// <param name="objects">The <see cref="JsonObject"/> instances in the array. If not set to, then the default
        /// value will be an empty <see cref="List{JsonObject}"/>.</param>
        public JsonArray(string? name = null, JsonObject? parent = null, IList<JsonObject>? objects = null) =>
            (Name, Parent, Objects) = (name, parent, objects ?? new List<JsonObject>());

        /// <summary>
        /// Creates a deep copy of this <see cref="JsonArray"/> instance. All elements in <see cref="Objects"/> are
        /// also copied.
        /// </summary>
        /// <returns>A new instance of <see cref="JsonArray"/> whose data is identical but separate from this instance.
        /// </returns>
        /// <remarks>The <see cref="Parent"/> of the new instance will be the same as this one.</remarks>
        public object Clone() => new JsonArray(Name, Parent, Objects.MemberClone());

        public IEnumerator<JsonObject> GetEnumerator() => Objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Objects.GetEnumerator();

        public void Add(JsonObject item) => Objects.Add(item);

        public void Clear() => Objects.Clear();

        public bool Contains(JsonObject item) => Objects.Contains(item);

        public void CopyTo(JsonObject[] array, int arrayIndex) => Objects.CopyTo(array, arrayIndex);

        public bool Remove(JsonObject item) => Objects.Remove(item);

        public int Count => Objects.Count;
        
        public bool IsReadOnly => Objects.IsReadOnly;

        public JsonObject this[int i]
        {
            get => Objects[i];
            set => Objects[i] = value;
        }
    }
}