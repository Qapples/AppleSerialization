using System;
using System.Text;
using Microsoft.Xna.Framework;

namespace AppleSerialization
{
    public static class ParseHelper
    {
        public static bool IsVectorDigit(char c) => c == '.' || char.IsDigit(c);

        public static bool TryParseVector2(in string s, out Vector2 value)
        {
            value = Vector2.Zero;

            //index of the space
            int i;
            for (i = 0; i < s.Length && s[i] != ' '; i++) ;

            if (i == s.Length - 1) return false;

            ReadOnlySpan<char> span = s.AsSpan();
            if (!float.TryParse(span[..i], out float x) || !float.TryParse(span[(i + 1)..], out float y))
            {
                return false;
            }

            value = new Vector2(x, y);
            return true;
        }

        public static bool TryParseVector3(in string s, out Vector3 value)
        {
            value = Vector3.Zero;

            //index of the spaces
            int i, i2;

            for (i = 0; i < s.Length && s[i] != ' '; i++) ;
            if (i == s.Length - 1) return false;

            for (i2 = i + 1; i2 < s.Length && s[i2] != ' '; i2++) ;
            if (i2 == s.Length - 1) return false;

            ReadOnlySpan<char> span = s.AsSpan();
            if (!float.TryParse(span[..i], out float x) ||
                !float.TryParse(span[(i + 1)..i2], out float y) ||
                !float.TryParse(span[(i2 + 1)..], out float z))
            {
                return false;
            }

            value = new Vector3(x, y, z);
            return true;
        }
    }
}