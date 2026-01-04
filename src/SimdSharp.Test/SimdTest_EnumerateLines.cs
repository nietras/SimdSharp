using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public sealed class SimdTest_EnumerateLines
{
    public record TestCase(string Name, string Text)
    {
        public override string ToString() => Name;
    }

    public static IEnumerable<TestCase> EnumerateLinesTestData =>
        [
            new("Empty", string.Empty),
            new("SingleLine", "single line"),
            new("LF", "line\nwith\nLF"),
            new("CR", "line\rwith\rCR"),
            new("CRLF", "line\r\nwith\r\nCRLF"),
            new("MixedLineEndings", "mix\r\nof\nline\rendings"),
            new("TrailingNewline", "trailing\nnewline\n"),
            new("StartsWithNewline", "\nstarts with newline"),
            new("QuotedLine", "\"quoted line\" and more"),
            new("RandomText2048", GenerateRandomText(seed: 1234, totalLength: 2_048, maxLineLength: 64))
        ];

    [TestMethod]
    [DynamicData(nameof(EnumerateLinesTestData))]
    public void SimdTest_EnumerateLines_(TestCase testCase)
    {
        var text = testCase.Text;

        // NOTE: BCL defines more than '\n','\r' as line breaks per:
        // https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs#L20
        //   public const string NewLineCharsExceptLineFeed = "\r\f\u0085\u2028\u2029";
        //   public static readonly SearchValues<char> NewLineChars =
        //     SearchValues.Create(NewLineCharsExceptLineFeed + "\n");
        // Hence, comparison is a bit unfair.
        var expectedEnumerator = MemoryExtensions.EnumerateLines(text);
        var actualEnumerator = Simd.EnumerateLines(text);

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
