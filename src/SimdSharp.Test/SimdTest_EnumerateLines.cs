using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public sealed class SimdTest_EnumerateLines
{
    public static IEnumerable<string> EnumerateLinesTestData =
        [
            string.Empty,
            "single line",
            "line\nwith\nLF",
            "line\rwith\rCR",
            "line\r\nwith\r\nCRLF",
            "mix\r\nof\nline\rendings",
            "trailing\nnewline\n",
            "\nstarts with newline",
            "\"quoted line\" and more",
            GenerateRandomText(seed: 1234, totalLength: 2_048, maxLineLength: 64)
        ];

    [TestMethod]
    [DynamicData(nameof(EnumerateLinesTestData))]
    public void SimdEnumerateLinesMatchesMemoryExtensions(string text)
    {
        //SpanLineEnumerator
        var expectedEnumerator = MemoryExtensions.EnumerateLines(text);
        var actualEnumerator = Simd.EnumerateLinesNew(text);

        while (Assert.AreEqualReturn(expectedEnumerator.MoveNext(), actualEnumerator.MoveNext()))
        {
            var expected = expectedEnumerator.Current;
            var actual = actualEnumerator.Current;
            Assert.AreEqual(expected.Length, actual.Length);
            Assert.AreSame(expected, actual);
        }
    }

    static string GenerateRandomText(int seed, int totalLength, int maxLineLength)
    {
        var random = new Random(seed);
        var sb = new StringBuilder(totalLength);
        var newlineVariants = new[] { "\n", "\r", "\r\n" };
        var count = 0;

        while (count < totalLength)
        {
            var lineLength = random.Next(1, maxLineLength + 1);
            if (count + lineLength > totalLength)
            {
                lineLength = totalLength - count;
            }

            if (lineLength > 0)
            {
                sb.Append('a', lineLength);
                count += lineLength;
            }

            if (count >= totalLength)
            {
                break;
            }

            var newline = newlineVariants[random.Next(newlineVariants.Length)];
            if (count + newline.Length > totalLength)
            {
                break;
            }

            sb.Append(newline);
            count += newline.Length;
        }

        return sb.ToString();
    }
}
