using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public unsafe class SimdTest_Parse_Details
{
    [DataRow(true, "12345678")]
    [DataRow(true, "123456789")]
    [DataRow(false, "1")]
    [DataRow(false, "12")]
    [DataRow(false, "123")]
    [DataRow(false, "1234")]
    [DataRow(false, "12345")]
    [DataRow(false, "123456")]
    [DataRow(false, "1234567")]
    [DataRow(false, "/1234567")]
    [DataRow(false, ":1234567")]
    [DataRow(false, "1:234567")]
    [DataRow(false, "1234567:")]
    [DataRow(false, "1234567 ")]
    [DataRow(false, "1234 567")]
    [TestMethod]
    public void SimdTest_Parse_Details_TryParseEightDigits_ManualCases(bool shouldPass, string sut)
    {
        Assert.InconclusiveIf(!Sse41.IsSupported);
        fixed (char* pos = sut)
        {
            Assert.AreEqual(shouldPass, TryParseEightDigits_Sse41(pos, out var res));
            if (shouldPass) { Assert.AreEqual(uint.Parse(sut.AsSpan()[..8]), res); }
        }
    }

    [TestMethod]
    public void SimdTest_Parse_Details_TryParseEightDigits_RandomCases()
    {
        Assert.InconclusiveIf(!Sse41.IsSupported);
        var random = new Random();
        for (var i = 0; i != 850000; i++)
        {
            var RandomNumber = random.Next(10000000, 99999999);
            var sut = RandomNumber.ToString();
            unsafe
            {
                fixed (char* pos = sut)
                {
                    Assert.IsTrue(TryParseEightDigits_Sse41(pos, out var res));
                    Assert.AreEqual(uint.Parse(sut), res);
                }
            }
        }
    }

    [TestMethod]
    public void SimdTest_Parse_Details_TryParseEightDigits_SameEightDigits()
    {
        Assert.InconclusiveIf(!Sse41.IsSupported);
        for (var i = 0; i <= 9; i++)
        {
            var sut = new string(i.ToString()[0], 8);
            unsafe
            {
                fixed (char* pos = sut)
                {
                    Assert.IsTrue(TryParseEightDigits_Sse41(pos, out uint res));
                    Assert.AreEqual(uint.Parse(sut), res);
                }
            }
        }
    }

    /// <summary>
    /// Detect eight consecutive digits and parse them a an unsigned int using SIMD instructions
    /// </summary>
    /// <param name="start">pointer to the sequence of char to evaluate</param>
    /// <param name="value">out : parsed value</param>
    /// <returns>bool : succes of operation : true meaning the sequence contains at least 8 consecutive digits</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParseEightDigits_Sse41(char* start, out uint value)
    {
        // escape if SIMD functions aren't available.
        if (!Sse41.IsSupported) { value = 0; return false; }

        Vector128<short> raw = Vector128.Load((short*)start); // Sse3.LoadDquVector128((short*)start);
        var ascii0 = Vector128.Create((short)(48 + short.MinValue));
        var after_ascii9 = Vector128.Create((short)(short.MinValue + 9));
        Vector128<short> a = Vector128.Subtract(raw, ascii0);
        Vector128<short> b = Vector128.LessThan(after_ascii9, a); // Sse2.CompareLessThan(after_ascii9, a);

        if (!Sse41.TestZ(b, b)) { value = 0; return false; }

        //  extract the low bytes of each 16-bit word
        if (false && Sse2.IsSupported)
        {
            // @Credit  AQRIT
            // https://stackoverflow.com/questions/66371621/hardware-simd-parsing-in-c-sharp-performance-improvement/66430672
            Vector128<byte> mul1 = Vector128.Create(0x14C814C8, 0x010A0A64, 0, 0).AsByte();
            Vector128<short> mul2 = Vector128.Create(0x00FA61A8, 0x0001000A, 0, 0).AsInt16();
            var vb = Vector128.Shuffle(a.AsByte(), Vector128.Create(0, 2, 4, 6, 8, 10, 12, 14, 0, 2, 4, 6, 8, 10, 12, 14).AsByte());
            Vector128<int> v = Sse2.MultiplyAddAdjacent(Ssse3.MultiplyAddAdjacent(mul1, vb.AsSByte()), mul2);
            v = Sse2.Add(Sse2.Add(v, v), Sse2.Shuffle(v, 1));
            value = (uint)v.GetElement(0);
        }
        //else if (AdvSimd.IsSupported)
        //{
        //    //value = (uint)v.GetElement(0);
        //}
        else
        {
            // https://stackoverflow.com/questions/66371621/simd-string-to-unsigned-int-parsing-in-c-sharp-performance-improvement/66430672#66430672

            var xmm = Sse2.LoadVector128((short*)start); // unsafe chunking
            var packed = Sse2.PackUnsignedSaturate(xmm, xmm); // convert digits from UTF16-LE to ASCII
            ulong val = Sse2.X64.ConvertToUInt64(packed.AsUInt64()); // extract to scalar
            //var packed = Vector128.NarrowWithSaturation(a.AsUInt16(), a.AsUInt16());
            //ulong val = packed.AsUInt64()[0]; // extract to scalar

            val -= 0x3030303030303030; // subtract '0' from each digit
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
