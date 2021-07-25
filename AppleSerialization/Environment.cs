using System;
using System.Collections.Generic;
using System.Text.Json;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleSerialization
{
    public static class Environment
    {
#nullable disable

        /// <summary>
        /// When a <see cref="FontSystem"/> is invalid, this one will be used instead in it's place.
        /// </summary>
        public static FontSystem DefaultFontSystem { get; set; }

        /// <summary>
        /// <see cref="GraphicsDevice"/> instance used for creating instances of <see cref="Texture2D"/> and more.
        /// </summary>
        public static GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// <see cref="RawContentManager"/> instance used for loading assets.
        /// </summary>
        public static RawContentManager ContentManager { get; set; }

        /// <summary>
        /// Represents the size of the object currently being deserialized. Used for generating Texture2D when a size
        /// is needed. If null, then no object has been deserialized. Only intended to be used in the Serializer class
        /// and not intended to be manipulated by users or other parts of the program
        /// </summary>
        // This is a very hacky and stupid solution and there may or may not be a better solution, but it works.
        public static Vector2? CurrentDeserializingObjectSize { get; set; }

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> contains any external types that will be involved in serialization.
        /// The key is the name of the type in string form.
        /// </summary>
        public static Dictionary<string, Type> ExternalTypes { get; } = new();
        
        /// <summary>
        /// Default <see cref="JsonSerializerOptions"/> instance for use in any serialization methods that accepts such
        /// a parameter.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultSerializerOptions = new()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public static readonly JsonWriterOptions DefaultWriterOptions = new() {Indented = true};
#nullable enable
    }
}