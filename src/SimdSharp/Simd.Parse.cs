using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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

    /// <summary>
    /// Detect eight consecutive digits and parse them a an unsigned int using SIMD instructions
    /// </summary>
    /// <param name="start">pointer to the sequence of char to evaluate</param>
    /// <param name="value">out : parsed value</param>
    /// <returns>bool : succes of operation : true meaning the sequence contains at least 8 consecutive digits</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe static bool TryParseEightDigits_Sse41(char* start, out uint value)
    {
        // escape if SIMD functions aren't available.
        //if (!Vector128.IsHardwareAccelerated) { value = 0; return false; }

        Vector128<short> raw = Vector128.Load((short*)start); // Sse3.LoadDquVector128((short*)start);
        var ascii0 = Vector128.Create((short)(48 + short.MinValue));
        var after_ascii9 = Vector128.Create((short)(short.MinValue + 9));
        Vector128<short> a = Vector128.Subtract(raw, ascii0);
        Vector128<short> b = Vector128.LessThan(after_ascii9, a); // Sse2.CompareLessThan(after_ascii9, a);

        //if (!Sse41.TestZ(b, b)) { value = 0; return false; }
        if (!Vector128.All(b, (byte)0)) { value = 0; return false; }

        if (Sse2.IsSupported)
        {
            //  extract the low bytes of each 16-bit word
            // @Credit  AQRIT
            // https://stackoverflow.com/questions/66371621/hardware-simd-parsing-in-c-sharp-performance-improvement/66430672
            Vector128<byte> mul1 = Vector128.Create(0x14C814C8, 0x010A0A64, 0, 0).AsByte();
            Vector128<short> mul2 = Vector128.Create(0x00FA61A8, 0x0001000A, 0, 0).AsInt16();
            var vb = Vector128.Shuffle(a.AsByte(), Vector128.Create(0, 2, 4, 6, 8, 10, 12, 14, 0, 2, 4, 6, 8, 10, 12, 14).AsByte());
            Vector128<int> v = Sse2.MultiplyAddAdjacent(Ssse3.MultiplyAddAdjacent(mul1, vb.AsSByte()), mul2);
            v = Sse2.Add(Sse2.Add(v, v), Sse2.Shuffle(v, 1));
            value = (uint)v.GetElement(0);
        }
        else
        {
            // Cross-platform based on narrow with saturation

            // https://stackoverflow.com/questions/66371621/simd-string-to-unsigned-int-parsing-in-c-sharp-performance-improvement/66430672#66430672

            var rawu16 = raw.AsUInt16();
            //var rawu16 = a.AsUInt16();
            var a2 = Vector128.Subtract(rawu16, Vector128.Create((ushort)(48)));
            //var b2 = Vector128.Subtract(rawu16, Vector128.Create((ushort)(48 + 9)));
            var packed2 = Vector128.NarrowWithSaturation(a2, a2);
            var val = packed2.AsUInt64()[0];

            //var packed = Vector128.NarrowWithSaturation(rawu16, rawu16);
            //var val = packed.AsUInt64()[0];

            //var packed = Sse2.PackUnsignedSaturate(raw, raw); // convert digits from UTF16-LE to ASCII
            //ulong val = Sse2.X64.ConvertToUInt64(packed.AsUInt64()); // extract to scalar
            //var packed = Vector128.NarrowWithSaturation(a.AsUInt16(), a.AsUInt16());
            //ulong val = packed.AsUInt64()[0]; // extract to scalar

            //val -= 0x3030303030303030; // subtract '0' from each digit
            //val <<= ((8 - text.Length) * 8); // shift off non-digit trash

            // convert
            const ulong mask = 0x000000FF000000FF;
            const ulong mul1s = 0x000F424000000064; // 100 + (1000000ULL << 32)
            const ulong mul2s = 0x0000271000000001; // 1 + (10000ULL << 32)
            val = (val * 10) + (val >> 8);
            val = (((val & mask) * mul1s) + (((val >> 16) & mask) * mul2s)) >> 32;
            value = (uint)val;
        }

        return true;

    }
}
