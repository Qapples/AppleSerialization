using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using AppleSerialization.Json;
using Microsoft.Xna.Framework;

namespace AppleSerialization
{
    /// <summary>
    /// Provides additional informatoin/data/context to aid in the serialization/deserialization process.
    /// </summary>
    public sealed class SerializationSettings : IDisposable
    {
        /// <summary>
        /// If a <see cref="JsonProperty"/> instance whose <see cref="JsonProperty.Name"/> is this value then that
        /// property represents the type of the object.
        /// </summary>
        public const string TypeIdentifier = "$type";

        private Dictionary<Type, object> _converters;
        
        /// <summary>
        /// Readonly dictionary of objects that implement <see cref="JsonConverter{T}"/> that are used
        /// for converting types during deserialization. The key is the <see cref="Type"/> of the converter, and the
        /// value is the converter object itself that implements <see cref="JsonConverter{T}"/>.
        /// </summary>
        public IReadOnlyDictionary<Type, object> Converters => _converters;
        
        /// <summary>
        /// The absolute directory that contains all the potential assets that can be used. This is necessary as some
        /// types that can be deserialized require external resources to be created (i.e.
        /// <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> or <see cref="FontStashSharp.FontSystem"/>).
        /// </summary>
        public string ContentDirectory { get; }
        
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> contains any external types that will be involved in serialization.
        /// The key is the name of the type in string form.
        /// </summary>
        public Dictionary<string, Type> ExternalTypes { get;  }

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> that provides alternative aliases for types. The key represents
        /// the alias for a type and the resulting value is the true, working name for that type.
        /// </summary>
        public Dictionary<string, string> TypeAliases { get; }

        /// <summary>
        /// This value is only relevant in the deserialization of UI elements. It is a <see cref="Vector2"/> represents
        /// the size of the current UI element being deserialize. It is necessary to keep track of this in order to
        /// generate missing texture place holders that are the appropriate size (yes, it's a bit of hack).
        /// </summary>
        internal Vector2 CurrentDeserializingObjectSize;

        public SerializationSettings(object[] converters, string contentDirectory,
            Dictionary<string, Type> externalTypes, Dictionary<string, string> typeAliases)
        {
            _converters = new Dictionary<Type, object>();
            foreach (object converter in converters)
            {
                _converters[converter.GetType()] = converter;
            }

            ContentDirectory = contentDirectory;
            ExternalTypes = externalTypes;
            TypeAliases = typeAliases;
            
            ValidateConverters();
        }

        private void ValidateConverters()
        {
            StringBuilder invalidConverterTypes = new();
            
            foreach (object converter in _converters.Values)
            {
                Type converterType = converter.GetType();
                
                if (!converterType.IsSubclassOf(typeof(JsonConverter<>)))
                {
                    invalidConverterTypes.Append($"{converterType.Name}, ");
                }
            }

            if (invalidConverterTypes.Length > 0)
            {
                throw new ArrayTypeMismatchException(
                    $"The following types do not implement JsonConverter<>: {invalidConverterTypes.ToString()[..^2]}.");
            }
        }

        public void Dispose()
        {
            foreach (object converterObj in _converters.Values)
            {
                if (converterObj is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            _converters.Clear();
            ExternalTypes.Clear();
            TypeAliases.Clear();
        }
    }
}