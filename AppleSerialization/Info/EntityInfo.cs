using System.Text.Json.Serialization;
using DefaultEcs;

namespace AppleSerialization.Info
{
    /// <summary>
    /// Describes the data needed for creating entities.
    /// </summary>
    public class EntityInfo
    {
        /// <summary>
        /// The ID of an <see cref="Entity"/> when it's created through <see cref="World.CreateEntity"/>.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The components of an <see cref="Entity"/> when it's created through <see cref="World.CreateEntity"/>.
        /// </summary>
        public object[] Components { get; set; }
        
        /// <summary>
        /// Constructs an <see cref="EntityInfo"/> instance (intended to be used in a Json context).
        /// </summary>
        /// <param name="id">The ID of the <see cref="Entity"/>.
        /// </param>
        /// <param name="components">The components of the <see cref="Entity"/></param>
        [JsonConstructor]
        public EntityInfo(string id, object[] components)
        {
            (Id, Components) = (id, components);
        }
    }
}