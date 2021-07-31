using System.Text.Json.Serialization;

namespace AppleSerialization
{
    /// <summary>
    /// Defines the path of content files (textures, models, etc.)
    /// </summary>
    public readonly struct ContentPath
    {
        /// <summary>
        /// The path of the content file. <br/>
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// If this value is true, then the <see cref="Path"/> value will be interpreted as
        /// <see cref="ContentPath"/>/<see cref="Path"/>. If false, then <see cref="Path"/> will be treated
        /// as absolute.
        /// </summary>
        public readonly bool IsContentPath;

        [JsonConstructor]
        public ContentPath(string path, bool isContentPath) => (Path, IsContentPath) = (path, isContentPath);
    }
}