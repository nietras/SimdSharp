using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace SimdSharp.ComparisonBenchmarks;

[MemoryDiagnoser]
[WarmupCount(3)]
[MinIterationCount(3)]
[MaxIterationCount(7)]
public unsafe class TryParseEightDigits
{
    [ParamsSource(nameof(Texts))]
    public string Text { get; set; } = Texts().First();
    // csFastFloat fails with thousands separator!
    public static IEnumerable<string> Texts() => ["12345678"]; //["1,234,567.890"];

    public TryParseEightDigits()
    {
        foreach (var t in Texts())
        {
            Text = t;
            var f0 = TryParseEightDigits_SimdSharp();
            var f1 = TryParseEightDigits_csFastFloat();
            var f2 = TryParseEightDigits_BCL();
            if (f0 != f1 || f0 != f2)
            {
                throw new ArgumentException($"{t} {f1}");
            }
        }
        Text = Texts().First();
    }

    [Benchmark()]
    public bool TryParseEightDigits_SimdSharp()
    {
        fixed (char* chars = Text)
        {
            return Simd.TryParseEightDigits_Sse41(chars, out var value);
        }
    }

    [Benchmark()]
    public bool TryParseEightDigits_csFastFloat()
    {
        fixed (char* chars = Text)
        {
            return TryParseEightConsecutiveDigits_SIMD(chars, out var value);
        }
    }

    // Not fair comparison, but just for baseline.
    [Benchmark(Baseline = true)]
    public bool TryParseEightDigits_BCL() => uint.TryParse(Text.AsSpan(), out var value);

    //[UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "TryParseEightConsecutiveDigits_SIMD")]
    //unsafe extern static bool TryParseEightConsecutiveDigits_SIMD(char* start, out uint value);
    /// <summary>
    /// Detect eight consecutive digits and parse them a an unsigned int using SIMD instructions
    /// </summary>
    /// <param name="start">pointer to the sequence of char to evaluate</param>
    /// <param name="value">out : parsed value</param>
    /// <returns>bool : succes of operation : true meaning the sequence contains at least 8 consecutive digits</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe static bool TryParseEightConsecutiveDigits_SIMD(char* start, out uint value)
    {

        // escape if SIMD functions aren't available.
        if (!Sse41.IsSupported)
        {
            return uint.TryParse(new ReadOnlySpan<char>(start, 8), out value);
            value = 0;
            return false;
        }


        value = 0;
        Vector128<short> raw = Sse41.LoadDquVector128((short*)start);
        Vector128<short> ascii0 = Vector128.Create((short)(48 + short.MinValue));
        Vector128<short> after_ascii9 = Vector128.Create((short)(short.MinValue + 9));
        Vector128<short> a = Sse41.Subtract(raw, ascii0);
        Vector128<short> b = Sse41.CompareLessThan(after_ascii9, a);

        if (!Sse41.TestZ(b, b))
        {
            return false;
        }

        // @Credit  AQRIT
        // https://stackoverflow.com/questions/66371621/hardware-simd-parsing-in-c-sharp-performance-improvement/66430672
        Vector128<byte> mul1 = Vector128.Create(0x14C814C8, 0x010A0A64, 0, 0).AsByte();
        Vector128<short> mul2 = Vector128.Create(0x00FA61A8, 0x0001000A, 0, 0).AsInt16();

        //  extract the low bytes of each 16-bit word
        var vb = Sse41.Shuffle(a.AsByte(), Vector128.Create(0, 2, 4, 6, 8, 10, 12, 14, 0, 2, 4, 6, 8, 10, 12, 14).AsByte());
        Vector128<int> v = Sse2.MultiplyAddAdjacent(Ssse3.MultiplyAddAdjacent(mul1, vb.AsSByte()), mul2);
        v = Sse2.Add(Sse2.Add(v, v), Sse2.Shuffle(v, 1));
        value = (uint)v.GetElement(0);

        return true;

    }
}
