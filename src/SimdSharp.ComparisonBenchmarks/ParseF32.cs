using System;
using System.Collections.Generic;
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
    public string Text { get; set; } = "1,234,567.890";
    public IEnumerable<string> Texts() => ["1,234,567.890"];

    [Benchmark()]
    public float ParseF32_SimdSharp() => float.ParseSimd(Text.AsSpan(), provider: null);

    [Benchmark()]
    public float ParseF32_csFastFloat() => FastFloatParser.ParseFloat(Text.AsSpan());

    [Benchmark(Baseline = true)]
    public float ParseF32_BCL() => float.Parse(Text.AsSpan(), provider: null);
}
