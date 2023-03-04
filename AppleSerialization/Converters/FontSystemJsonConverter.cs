using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using FontStashSharp;

namespace AppleSerialization.Converters
{
    /// <summary>
    /// Converts the name of a font system to a <see cref="FontSystem"/> instance when deserializing a json file.
    /// </summary>
    public class FontSystemJsonConverter : JsonConverter<FontSystem>, IDisposable
    {
        public SerializationSettings SerializationSettings { get; init; }
        public FontSystem DefaultFontSystem { get; init; }

        private Dictionary<string, FontSystem> _fontSystemsCache;
        public IReadOnlyDictionary<string, FontSystem> FontSystemsCache => _fontSystemsCache;

        public FontSystemJsonConverter(SerializationSettings settings, FontSystem defaultFontSystem)
        {
            (SerializationSettings, DefaultFontSystem) = (settings, defaultFontSystem);

            _fontSystemsCache = new Dictionary<string, FontSystem>();
        }
        
        /// <summary>
        /// Given a directory relative <see cref="SerializationSettings.ContentDirectory"/> provided by a
        /// <see cref="Utf8JsonReader"/>, creates a <see cref="FontSystem"/> using the font files existing under that
        /// directory.
        /// </summary>
        /// <param name="reader">JsonReader instance used to read data from the Json file. The data should be in the
        /// from of an array, and the first element represents the name to load the fonts under.</param>
        /// <param name="typeToConvert">The type that is being converted to (FontSystem).</param>
        /// <param name="options">The options of the JsonSerializer used.</param>
        /// <returns>If successful, a FontSystem that contains the loaded fonts from the specified directory is
        /// returned. If unsuccessful, <see cref="DefaultFontSystem"/> is returned and a message to the debug console
        /// is written to.</returns>
        public override FontSystem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            const string methodName = $"{nameof(FontSystemJsonConverter)}.{nameof(Read)}";
#endif
            string? fontSystemRelativePath = reader.GetString();
            if (fontSystemRelativePath is null)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: cannot get string value during reading Returning DefaultFontSystem");
#endif
                return DefaultFontSystem;
            }

            string fontSystemAbsolutePath =
                Path.Combine(SerializationSettings.ContentDirectory, fontSystemRelativePath);

            if (!Directory.Exists(fontSystemAbsolutePath))
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: directory to font system does not exist. Returning " +
                                $"DefaultFontSystem. Requested directory: {fontSystemAbsolutePath}");
#endif
                return DefaultFontSystem;
            }

            string[] fontFileDirectories = Directory.GetFiles(fontSystemAbsolutePath, "*.ttf");
            if (fontFileDirectories.Length == 0)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: cannot find font files with .ttf extension in directory. " +
                                $"Returning DefaultFontSystem. Requested directory: {fontSystemAbsolutePath}");
#endif
                return DefaultFontSystem;
            }

            if (_fontSystemsCache.TryGetValue(fontSystemRelativePath, out FontSystem? cachedFontSystem))
            {
                return cachedFontSystem;
            }

            FontSystem fontSystem = new();
            foreach (string fontFileDirectory in fontFileDirectories)
            {
                fontSystem.AddFont(File.ReadAllBytes(fontFileDirectory));
            }

            _fontSystemsCache[fontSystemRelativePath] = fontSystem;
            return fontSystem;
        }

        /// <summary>
        /// SpriteFonts are not intended to be written as, therefore, this method will always return a
        /// NotImplementedException
        /// </summary>
        public override void Write(Utf8JsonWriter writer, FontSystem value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (FontSystem fontSystem in _fontSystemsCache.Values)
            {
                fontSystem.Dispose();
            }
            
            _fontSystemsCache.Clear();
        }
    }
}