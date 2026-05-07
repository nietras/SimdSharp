using System;
using System.Collections.Generic;
using System.Linq;
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
            return FastFloatAccessor.TryParseEightConsecutiveDigits_SIMD(null, chars, out var value);
        }
    }

    // Not fair comparison, but just for baseline.
    [Benchmark(Baseline = true)]
    public bool TryParseEightDigits_BCL() => uint.TryParse(Text.AsSpan(), out var value);
}
