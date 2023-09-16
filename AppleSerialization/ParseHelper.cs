using System;
using System.Text;
using Microsoft.Xna.Framework;

namespace AppleSerialization
{
    public static class ParseHelper
    {
        public static bool TryParseVector(string s, ref Span<float> values)
        {
            int len = values.Length;

            Span<int> indices = stackalloc int[len - 1];
            for (int i = 0; i < len - 1; i++)
            {
                //travel the string until the next space and store the index of that space in c
                int c;
                for (c = i == 0 ? 0 : indices[i - 1] + 1; c < s.Length && s[c] != ' '; c++) ;

                //ensure that there are "len" amount of values in the string separated by spaces.
                if (i != len - 2 && c == s.Length - 2) return false;

                indices[i] = c;
            }

            ReadOnlySpan<char> strSpan = s.AsSpan();
            for (int i = 0; i < len - 1; i++)
            {
                int spaceIndex = indices[i];

                if (i == 0)
                {
                    if (!float.TryParse(strSpan[..spaceIndex], out float value))
                    {
                        return false;
                    }

                    values[i] = value;
                }
                else
                {
                    int prevIndex = indices[i - 1];
                    
                    if (!float.TryParse(strSpan[(prevIndex + 1)..spaceIndex], out float value))
                    {
                        return false;
                    }

                    values[i] = value;
                }
            }
            
            //edge case
            int lastIndex = indices[len - 2];

            if (lastIndex == s.Length) return false;

            if (!float.TryParse(strSpan[(lastIndex + 1)..], out float lastVal))
            {
                return false;
            }

            values[len - 1] = lastVal;

            return true;
        }

        public static bool TryParseVector2(string s, out Vector2 value)
        {
            Span<float> values = stackalloc float[2];

            if (!TryParseVector(s, ref values))
            {
                value = Vector2.Zero;
                return false;
            }

            value = new Vector2(values[0], values[1]);
            return true;
        }
        
        public static bool TryParseVector3(string s, out Vector3 value)
        {
            Span<float> values = stackalloc float[3];

            if (!TryParseVector(s, ref values))
            {
                value = Vector3.Zero;
                return false;
            }

            value = new Vector3(values[0], values[1], values[2]);
            return true;
        }
        
        public static bool TryParseVector4(string s, out Vector4 value)
        {
            Span<float> values = stackalloc float[4];

            if (!TryParseVector(s, ref values))
            {
                value = Vector4.Zero;
                return false;
            }

            value = new Vector4(values[0], values[1], values[2], values[3]);
            return true;
        }

        public static bool TryParseColor(string s, out Color color)
        {
            if (!TryParseVector4(s, out Vector4 colorVec4) || byte.MaxValue < colorVec4.X ||
                byte.MaxValue < colorVec4.Y || byte.MaxValue < colorVec4.Z || byte.MaxValue < colorVec4.W)
            {
                color = Color.Transparent;
                return false;
            }

            color = new Color((byte) colorVec4.X, (byte) colorVec4.Y, (byte) colorVec4.Z, (byte) colorVec4.W);
            return true;
        }
    }
}