using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Audio;
using MonoSound;

namespace AppleSerialization.Converters
{
    public class SoundEffectJsonConverter : JsonConverter<SoundEffect>, IFromStringConverter, IDisposable
    {
        public IReadOnlyDictionary<string, SoundEffect> SoundEffectCache => _soundEffectCache;
        private readonly Dictionary<string, SoundEffect> _soundEffectCache;

        private SerializationSettings _serializationSettings;

        private static readonly SoundEffect BlankSoundEffect = new(new byte[2], 8000, AudioChannels.Mono);
        
        public SoundEffectJsonConverter(SerializationSettings settings)
        {
            _serializationSettings = settings;
            _soundEffectCache = new Dictionary<string, SoundEffect>();
        }

        public override SoundEffect? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            const string methodName = $"{nameof(SoundEffectJsonConverter)}.{nameof(Read)}";

            string? soundEffectRelativePath = reader.GetString();
            if (soundEffectRelativePath is null)
            {
                Debug.WriteLine($"{methodName}: cannot get string value during reading. Returning blank audio.");
                return BlankSoundEffect;
            }

            return ConvertStringToSoundEffect(soundEffectRelativePath);
        }

        public override void Write(Utf8JsonWriter writer, SoundEffect value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private SoundEffect ConvertStringToSoundEffect(string soundEffectRelativePath)
        {
            const string methodName = $"{nameof(SoundEffectJsonConverter)}.{nameof(ConvertStringToSoundEffect)}";

            string soundEffectAbsolutePath =
                Path.Combine(_serializationSettings.ContentDirectory, soundEffectRelativePath);

            if (!File.Exists(soundEffectAbsolutePath))
            {
                Debug.WriteLine($"{methodName}: cannot find sound effect at path {soundEffectAbsolutePath}. " +
                                $"Using blank sound effect.");
                return BlankSoundEffect;
            }

            if (_soundEffectCache.TryGetValue(soundEffectRelativePath, out SoundEffect? cachedSoundEffect))
            {
                return cachedSoundEffect;
            }

            using FileStream soundEffectStream = new(soundEffectAbsolutePath, FileMode.Open, FileAccess.Read);

            return _soundEffectCache[soundEffectRelativePath] =
                EffectLoader.GetEffect(soundEffectStream, AudioType.OGG);
        }
        
        public object ConvertFromString(string soundEffectRelativePath) =>
            ConvertStringToSoundEffect(soundEffectRelativePath);

        public void Dispose()
        {
            foreach (SoundEffect soundEffect in _soundEffectCache.Values)
            {
                soundEffect.Dispose();
            }
            
            _soundEffectCache.Clear();
        }
    }
}