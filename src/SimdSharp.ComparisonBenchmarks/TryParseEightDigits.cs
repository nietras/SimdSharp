using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public static IEnumerable<string> Texts() => ["12345678"];

    public TryParseEightDigits()
    {
        foreach (var t in Texts())
        {
            Text = t;
            var v0 = TryParseEightDigits_SimdSharp();
            var v1 = TryParseEightDigits_csFastFloat();
            var v2 = TryParseEightDigits_BCL();
            if (v0 != v1 || v0 != v2)
            {
                throw new ArgumentException($"{t} {v1}");
            }
        }
        Text = Texts().First();
    }

    [Benchmark()]
    public uint TryParseEightDigits_SimdSharp()
    {
        Simd.TryParseEightDigits(ref MemoryMarshal.GetReference(Text.AsSpan()), out var value);
        return value;
    }

    [Benchmark()]
    public uint TryParseEightDigits_csFastFloat()
    {
        if (!Sse41.IsSupported) { return TryParseEightDigits_BCL(); }
        fixed (char* chars = Text)
        {
            FastFloatAccessor.TryParseEightConsecutiveDigits_SIMD(null, chars, out var value);
            return value;
        }
    }

    // Not fair comparison, but just for baseline.
    [Benchmark(Baseline = true)]
    public uint TryParseEightDigits_BCL()
    {
        uint.TryParse(Text.AsSpan(), out var value);
        return value;
    }
}
