using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Attributes;
using csFastFloat;

namespace SimdSharp.ComparisonBenchmarks;

[MemoryDiagnoser]
[WarmupCount(3)]
[MinIterationCount(3)]
[MaxIterationCount(7)]
public class ParseF32
{
    [ParamsSource(nameof(Texts))]
    public string Text { get; set; } = Texts().First();
    // csFastFloat fails with thousands separator!
    public static IEnumerable<string> Texts() => ["1234567.890"]; //["1,234,567.890"];

    public ParseF32()
    {
        foreach (var t in Texts())
        {
            Text = t;
            var f0 = ParseF32_SimdSharp();
            var f1 = ParseF32_csFastFloat();
            var f2 = ParseF32_BCL();
            if (f0 != f1 || f0 != f2)
            {
                throw new ArgumentException($"{t} {f1}");
            }
        }
        Text = Texts().First();
    }

    [Benchmark()]
    public float ParseF32_SimdSharp() => float.ParseSimd(Text.AsSpan(), provider: null);

    [Benchmark()]
    public float ParseF32_csFastFloat() => FastFloatParser.ParseFloat(Text.AsSpan(), NumberStyles.Float, decimal_separator: '.');

    [Benchmark(Baseline = true)]
    public float ParseF32_BCL() => float.Parse(Text.AsSpan(), provider: null);
}
