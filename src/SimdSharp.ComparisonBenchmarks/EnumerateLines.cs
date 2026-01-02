using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace SimdSharp.ComparisonBenchmarks;

[MemoryDiagnoser]
[EvaluateOverhead(false)]
[WarmupCount(3)]
[MinIterationCount(3)]
[MaxIterationCount(7)]
public class EnumerateLinesSpanUTF8
{
    string m_text = "";

    [GlobalSetup]
    public void GlobalSetup()
    {
        m_text = GenerateText();
    }

    //[Benchmark]
    public void ReadLine_()
    {
        using var reader = new StringReader(m_text);
        var sum = 0;
        while (reader.ReadLine() is { } line)
        {
            sum += line.Length;
        }
    }

    [Benchmark]
    public void EnumerateLines_BCL()
    {
        var lines = MemoryExtensions.EnumerateLines(m_text);
        var sum = 0;
        foreach (var line in lines)
        {
            sum += line.Length;
        }
    }

    [Benchmark]
    public void EnumerateLines_SimdSharp()
    {
        var sum = 0;
        foreach (var line in Simd.EnumerateLines(m_text))
        {
            sum += line.Length;
        }
    }

    static string GenerateText(int totalLength = 1024 * 1024, int maxLineLength = 100)
    {
        var rnd = new Random(42);
        var sb = new StringBuilder(capacity: totalLength);
        var count = 0;
        while (count < totalLength)
        {
            var lineLength = rnd.Next(1, maxLineLength + 1);
            if (count + lineLength + 1 > totalLength)
            {
                lineLength = totalLength - count - 1;
                if (lineLength <= 0) break;
            }
            sb.Append((char)rnd.Next('a', 'z' + 1), lineLength);
            sb.Append('\n');
            count += lineLength + 1;
        }
        return sb.ToString();
    }
}
