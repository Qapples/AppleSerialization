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
            
            MethodInfo? tryParse;
            object?[] args;
            
            if (valueType.BaseType == typeof(Enum))
            {
                tryParse = typeof(Enum).GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public,
                    new[] { typeof(Type), typeof(string), typeof(object).MakeByRefType() });
                args = new object?[] { valueType, Value, null };
            }
            else
            {
                tryParse = valueType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public,
                        new[] { typeof(string), valueType.MakeByRefType() });
                args = new object?[] { Value, null };
            }

            if (tryParse is null) return false;
    
            bool success = (bool?) tryParse.Invoke(null, args) ?? false;
            if (success)
            {
                // last index of a TryParse method is always the output parameter.
                valueObj = args[^1];
                return true;
            }
    
            return false;
        }
    }
}