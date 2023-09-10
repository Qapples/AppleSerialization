using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleSerialization.Converters
{
    /// <summary>
    /// Converts a string representative of a <see cref="Texture2D"/> instance when deserializing a json file.
    /// </summary>
    public class Texture2DJsonConverter : JsonConverter<Texture2D>, IFromStringConverter, IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; init; }
        public SerializationSettings SerializationSettings { get; init; }

        private Dictionary<string, Texture2D> _textureCache;
        private IReadOnlyDictionary<string, Texture2D> TextureCache => _textureCache;

        public Texture2DJsonConverter(GraphicsDevice graphicsDevice,
            SerializationSettings settings)
        {
            (GraphicsDevice, SerializationSettings) = (graphicsDevice, settings);

            _textureCache = new Dictionary<string, Texture2D>();
        }
        
        /// <summary>
        /// Given the relative path (relative to <see cref="SerializationSettings.ContentDirectory"/>) of a texture,
        /// or a <see cref="Color"/> in the format of "Color.{Color Name}", returns a <see cref="Texture2D"/> object
        /// from a string value in json files.
        /// </summary>
        /// <param name="reader">JsonReader instance used to read data from the Json file</param>
        /// <param name="typeToConvert">The type that is being converted to (Texture2D)</param>
        /// <param name="options">The options of the JsonSerializer used</param>
        /// <returns>The Texture2D found by a relative path or the name of a <see cref="Color"/>. If the string value from
        /// the Json object begins with "Color.", then a width * height box which is homogenous of the color specified
        /// is returned as a Texture. If no Texture2D is found, then a black and pink checkerboard texture is used in
        /// place. If there is no size or scale property found within the Json object, a default size of 100x100 is used.
        /// (Bounds property is prioritized if both size and scale property exists)</returns>
        public override Texture2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            const string methodName = $"{nameof(Texture2DJsonConverter)}.{nameof(Read)}";
#endif
            string? textureRelativePath = reader.GetString();
            GetCurrentObjectSize(out var size);
            
            if (textureRelativePath is null)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: Unable to get the string value when getting Texture2D. Using " +
                                $"default texture.");
#endif
                return TextureHelper.GenerateDefaultTexture(GraphicsDevice, size.Width, size.Width);
            }

            return ConvertFromStringToTexture(textureRelativePath);
        }

        /// <summary>
        /// Due to limitations and other roadblocks (such as the difficultly of expressing Texture data compactly in
        /// Json format) and the fact that this method will not be used often since most Json will be written by hand,
        /// this method is not implemented and will throw a NotImplementedException exception
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Texture2D value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        
        public Texture2D ConvertFromStringToTexture(string textureRelativePath)
        {
#if DEBUG
            const string methodName = $"{nameof(Texture2DJsonConverter)}.{nameof(ConvertFromStringToTexture)}";
#endif
            if (!GetCurrentObjectSize(out var size))
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: unable to obtain current object size. The generated color or default" +
                                $" texture will have a dimension of 1x1.");
#endif
            }
            
            //First, try to parse a color value. If there is no color value, then get a Texture2D by name via the global
            //content manager
            
            Color? textureColor = TextureHelper.GetColorFromName(textureRelativePath);

            if (textureColor is not null)
            {
                Color paramColor = textureColor.Value;

                //TextureFromColor is not case-sensitive, so we don't need to do anything extra to the string value
                return TextureHelper.CreateTextureFromColor(GraphicsDevice, paramColor, size.Width, size.Height);
            }

            //the string value is not a color, so intercept it as a name to a Texture2D existing within the content
            //manager
            if (_textureCache.TryGetValue(textureRelativePath, out Texture2D? cachedTexture))
            {
                return cachedTexture;
            }

            string textureAbsolutePath = Path.Combine(SerializationSettings.ContentDirectory, textureRelativePath);
            if (File.Exists(textureAbsolutePath))
            {
                Texture2D texture = Texture2D.FromFile(GraphicsDevice, textureAbsolutePath);
                _textureCache[textureRelativePath] = texture;

                return texture;
            }

#if DEBUG
            Debug.WriteLine($"{methodName}: cannot find texture from path. Returning default texture. Requested" +
                            $" path: {textureAbsolutePath}");
#endif
            return TextureHelper.GenerateDefaultTexture(GraphicsDevice, size.Width, size.Height);
        }

        public object ConvertFromString(string textureRelativePath) => ConvertFromStringToTexture(textureRelativePath);

        private bool GetCurrentObjectSize(out (int Width, int Height) size)
        {
            Vector2? currentObjSize = SerializationSettings.CurrentDeserializingObjectSize;
            size = SerializationSettings.CurrentDeserializingObjectSizeType switch
            {
                "Ratio" or "ratio" => ((int) ((currentObjSize?.X ?? 1) * GraphicsDevice.Viewport.Width),
                    (int) ((currentObjSize?.Y ?? 1) * GraphicsDevice.Viewport.Width)),
                _ => ((int) (currentObjSize?.X ?? 1), (int) (currentObjSize?.Y ?? 1))
            };

            return currentObjSize is not null;
        }

        public void Dispose()
        {
            foreach (Texture2D texture in _textureCache.Values)
            {
                texture.Dispose();
            }
            
            _textureCache.Clear();
        }
    }
}