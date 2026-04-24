using System;
using System.Diagnostics.CodeAnalysis;

namespace SimdSharp;

public static partial class Simd
{
    extension<T>(T) where T : ISpanParsable<T>
    {
        public static T ParseSimd(ReadOnlySpan<char> s, [MaybeNullWhen(returnValue: false)] IFormatProvider? provider)
        {
            // TODO: Call TryParse for half,float, double

            return T.Parse(s, provider);
        }

        public static bool TryParseSimd(ReadOnlySpan<char> s, [MaybeNullWhen(returnValue: false)] IFormatProvider? provider, out T result)
        {
            // TODO: SIMD parse for half, float, double

            return T.TryParse(s, provider, out result!);
        }
    }
}
