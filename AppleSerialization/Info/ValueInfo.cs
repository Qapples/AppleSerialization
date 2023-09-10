using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace AppleSerialization.Info
{
    public readonly struct ValueInfo
    {
        public readonly string ValueType;
        public readonly string Value;

        [JsonConstructor]
        public ValueInfo(string valueType, string value) => (ValueType, Value) = (valueType, value);
        
        public bool TryGetValue(SerializationSettings settings, out object? valueObj)
        {
            valueObj = null;

            Type? valueType = ConverterHelper.GetTypeFromString(ValueType, settings);

            if (valueType is null) return false;

            JsonConverter? jsonConverter = settings.Converters.FirstOrDefault(c => c.Value.CanConvert(valueType)).Value;

            if (jsonConverter is IFromStringConverter fromStringConverter)
            {
                valueObj = fromStringConverter.ConvertFromString(Value);
                return true;
            }
            
            if (valueType == typeof(string))
            {
                valueObj = Value;
                return true;
            }
            
            MethodInfo? tryParse = valueType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public,
                new[] { typeof(string), valueType.MakeByRefType() });
            
            if (tryParse is null) return false;
    
            object?[] args = { Value, null };
            bool success = (bool?) tryParse.Invoke(null, args) ?? false;
            if (success)
            {
                valueObj = args[1];
                return true;
            }
    
            return false;
        }
    }
}