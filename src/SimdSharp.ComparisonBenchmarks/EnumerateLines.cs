using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace SimdSharp.ComparisonBenchmarks;

[MemoryDiagnoser]
[EvaluateOverhead(false)]
[WarmupCount(3)]
[MinIterationCount(3)]
[MaxIterationCount(7)]
public class EnumerateLinesSpanUTF16
{
    string m_text = "";

    [ParamsSource(nameof(TotalLengthParams))]
    public int TotalLength { get; set; }
    public IEnumerable<int> TotalLengthParams() => [32 * 1024];

    [ParamsSource(nameof(MaxLineLengthParams))]
    public int MaxLineLength { get; set; }
    public IEnumerable<int> MaxLineLengthParams() => [0, 8, 128];

    [GlobalSetup]
    public void GlobalSetup()
    {
        m_text = GenerateText(TotalLength, maxLineLength: MaxLineLength);
    }

    //[Benchmark]
    public void ReadLine_BCL()
    {
        using var reader = new StringReader(m_text);
        var sum = 0;
        while (reader.ReadLine() is { } line)
        {
            sum += line.Length;
        }
    }

    [Benchmark(Baseline = true)]
    public nint EnumerateLines_BCL()
    {
        nint sum = 0;
        foreach (var line in MemoryExtensions.EnumerateLines(m_text))
        {
            sum += line.Length;
        }
        return sum;
    }

    [Benchmark]
    public nint EnumerateLines_SimdSharp()
    {
        nint sum = 0;
        foreach (var line in Simd.EnumerateLines(m_text))
        {
            sum += line.Length;
        }
        return sum;
    }

    static string GenerateText(int totalLength = 1024 * 1024, int maxLineLength = 100)
    {
        var random = new Random(42);
        var sb = new StringBuilder(capacity: totalLength);
        var count = 0;
        while (count < totalLength)
        {
            var lineLength = random.Next(1, maxLineLength + 1);
            if (count + lineLength + 1 > totalLength)
            {
                lineLength = totalLength - count - 1;
                if (lineLength <= 0) break;
            }
            sb.Append('a', lineLength);
            sb.Append('\n');
            count += lineLength + 1;
        }
        return sb.ToString();
    }
}
