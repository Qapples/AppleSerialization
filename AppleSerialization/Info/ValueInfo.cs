using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AppleSerialization.Info
{
    public readonly struct ValueInfo
    {
        public readonly string ValueType;
        public readonly string Value;

        [JsonConstructor]
        public ValueInfo(string valueType, string value) => (ValueType, Value) = (valueType, value);
        
        public bool TryGetValue(out object? value)
        {
            value = null;

            Type? valueType = Type.GetType(ValueType);
            MethodInfo? tryParse = valueType?.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public,
                new[] { typeof(string), valueType.MakeByRefType() });

            if (valueType == typeof(string))
            {
                value = Value;
                return true;
            }

            if (tryParse is null)
            {
                return false;
            }
    
            object?[] args = { Value, null };
            bool success = (bool?) tryParse.Invoke(null, args) ?? false;
            if (success)
            {
                value = args[1];
                return true;
            }
    
            return false;
        }
    }
}