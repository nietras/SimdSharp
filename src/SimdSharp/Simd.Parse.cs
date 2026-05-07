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
    /// Detect eight consecutive digits and parse them as an unsigned int using
    /// SIMD instructions.
    /// </summary>
    /// <returns>true if sequence contains at least 8 consecutive digits,
    /// otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParseEightDigits(ref char startRef, out uint value)
    {
        // Adopted from csFastFloat but with cross-platform Vector128 migration
        // and support, so it works on ARM with NEON for example.

        var raw = Unsafe.ReadUnaligned<Vector128<short>>(ref Unsafe.As<char, byte>(ref startRef));
        // Sse41/2 as better support for signed 16-bit so using that here
        var ascii0 = Vector128.Create((short)(48 + short.MinValue)); // 48 = 0x30 = '0'
        var ascii9Plus1 = Vector128.Create((short)(short.MinValue + 9));
        var a = Vector128.Subtract(raw, ascii0);

        // Check all are digits
        var b = Vector128.LessThan(ascii9Plus1, a);
        if (!Vector128.All(b, (byte)0)) { value = 0; return false; }

        if (Sse2.IsSupported)
        {
            // extract the low bytes of each 16-bit word
            // @Credit  AQRIT
            // https://stackoverflow.com/questions/66371621/hardware-simd-parsing-in-c-sharp-performance-improvement/66430672
            Vector128<byte> mul1 = Vector128.Create(0x14C814C8, 0x010A0A64, 0, 0).AsByte();
            Vector128<short> mul2 = Vector128.Create(0x00FA61A8, 0x0001000A, 0, 0).AsInt16();
            var shuffles = Vector128.Create((byte)0, 2, 4, 6, 8, 10, 12, 14, 0, 2, 4, 6, 8, 10, 12, 14);
            var vb = Vector128.Shuffle(a.AsByte(), shuffles);
            Vector128<int> v = Sse2.MultiplyAddAdjacent(Ssse3.MultiplyAddAdjacent(mul1, vb.AsSByte()), mul2);
            v = Sse2.Add(Sse2.Add(v, v), Sse2.Shuffle(v, 1));
            value = (uint)v.GetElement(0);
        }
        else
        {
            // Cross-platform based on narrow with saturation and SWAR
            // https://stackoverflow.com/questions/66371621/simd-string-to-unsigned-int-parsing-in-c-sharp-performance-improvement/66430672#66430672

            // Convert SIMD to one 64-bit register with 8 x 8-bit "digits"
            var rawU16 = raw.AsUInt16();
            var zeroBased = Vector128.Subtract(rawU16, Vector128.Create((ushort)(48))); // 48 = 0x30 = '0'
            var saturated = Vector128.NarrowWithSaturation(zeroBased, zeroBased);
            // Sse2 has better instructions but appears not to be used by BCL for some reason for NarrowWithSaturation
            //var saturated = Sse2.PackUnsignedSaturate(zeroBased.AsInt16(), zeroBased.AsInt16()).AsByte();
            var val = saturated.AsUInt64()[0];

            value = ParseEightDigitsSWAR(val);
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint ParseEightDigitsSWAR(ulong val)
    {
        // Multiply and sum the digits
        const ulong mask = 0x000000FF000000FF;
        const ulong mul1 = 0x000F424000000064; // 100 + (1000000ULL << 32)
        const ulong mul2 = 0x0000271000000001; // 1 + (10000ULL << 32)
        val = (val * 10) + (val >> 8);
        var valMask = mask & val;
        var valShrMask = mask & (val >> 16);
        val = ((valMask * mul1) + (valShrMask * mul2)) >> 32;
        return (uint)val;
    }
}
