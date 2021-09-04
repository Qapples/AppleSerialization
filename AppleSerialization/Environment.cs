using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        /// A <see cref="Dictionary{TKey,TValue}"/> that provides alternative aliases for types. The key represents
        /// the alias for a type and the resulting value is the true, working name for that type.
        /// </summary>
        public static Dictionary<string, string> TypeAliases { get; } = new();
        
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

        /// <summary>
        /// Returns the full path of a <see cref="ContentPath"/> instance. If <see cref="ContentPath.IsContentPath"/>
        /// is true, then <see cref="ContentPath.Path"/> is combined with <see cref="RawContentManager.Directory"/>.
        /// </summary>
        /// <param name="path">The <see cref="ContentPath"/> instance to get the path from.</param>
        /// <returns>If <see cref="ContentPath.IsContentPath"/> is true and <see cref="RawContentManager"/> is null,
        /// then null is returned. Otherwise, a value is returned.</returns>
        public static string? GetFullPath(this in ContentPath path)
        {
            if (path.IsContentPath)
            {
                return ContentManager is not null
                    ? Path.GetFullPath(Path.Combine(ContentManager.Directory, path.Path))
                    : null;
            }

            return Path.GetFullPath(path.Path);
        }

        /// <summary>
        /// Parses the contents of a file detailing alternative aliases for types that will be present when loading
        /// entity files. The aliases can be found in the <see cref="TypeAliases"/> instance.
        /// </summary>
        /// <param name="fileContents">The CONTENTS of the file to parse.</param>
        public static void LoadTypeAliasFileContents(string fileContents)
        {
            foreach (Match match in Regex.Matches(fileContents, @"(\w+)\W+""([\w., ]+)"))
            {
                if (match.Groups.Count < 3) continue;

                TypeAliases.Add(match.Groups[1].Value.ToLower(), match.Groups[2].Value);
            }
        }
    }
}