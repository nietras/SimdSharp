using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public class SimdTest_Parse
{
    const int SignBitCount = 1;
    const int ExponentBitCount = 8;
    const int MantissaBitCount = 23;
    const int ExponentShift = MantissaBitCount;
    const int ExponentCount = 1 << ExponentBitCount;
    const int ExponentMask = ExponentCount - 1;
    const int MantissaMask = (1 << MantissaBitCount) - 1;
    const int MantissaMidpoint = 1 << (MantissaBitCount - 1);
    const int NegativeSignBit = unchecked((int)(1u << (MantissaBitCount + ExponentBitCount)));

    static ReadOnlySpan<int> SignBits => [0, NegativeSignBit];

    static string?[] Formats { get; } = [null, "G9", "R", "E9"];

    static CultureInfo?[] CultureInfos { get; } =
        [null, new(""), new("en-US"), new("fr-FR"), new("da-DK")];

    static string[] LeadingAffixes { get; } = ["", " ", "  ", "   ", "\t", "+"];

    static int MaxLeadingAffixLength { get; } = GetMaxLength(LeadingAffixes);

    static string[] TrailingAffixes { get; } = ["", " ", "  ", "   ", "\t"];

    static int MaxTrailingAffixLength { get; } = GetMaxLength(TrailingAffixes);

    static float[] ParseCoverageValues { get; } =
        [0f, 1f, -1f, 12.5f, -12.5f, 1234.5f, -1234.5f, 1234567f, 1.23e-4f, -1.23e-4f];

    static ReadOnlySpan<int> Mantissas =>
        [
            0,
            1,
            2,
            3,
            MantissaMidpoint - 1,
            MantissaMidpoint,
            MantissaMidpoint + 1,
            MantissaMask - 3,
            MantissaMask - 2,
            MantissaMask - 1,
            MantissaMask
        ];

    public static IEnumerable<Float32TestCase> Float32TestData { get; } = EnumerateFloat32TestData();

    public static IEnumerable<ParseTextCase> Float32CoverageData { get; } = EnumerateFloat32CoverageData();

    [TestMethod]
    [DynamicData(nameof(Float32TestData))]
    public void SimdTest_Parse_RoundTrip_BCL_(Float32TestCase testCase)
    {
        var v = testCase.Value;
        Span<char> parseBuffer = stackalloc char[1024 + MaxLeadingAffixLength + MaxTrailingAffixLength];

        foreach (var cultureInfo in CultureInfos)
        {
            foreach (var format in Formats)
            {
                var cultureName = cultureInfo?.Name ?? "";

                var formatDestination = parseBuffer[MaxLeadingAffixLength..];
                Assert.IsTrue(v.TryFormat(formatDestination, out var charsWritten,
                    format: format, provider: cultureInfo));

                foreach (var leading in LeadingAffixes)
                {
                    if (leading == "+" && float.IsNegative(v)) { continue; }

                    var leadingSpan = leading == "+" ? "+".AsSpan() : leading.AsSpan();
                    var leadingLength = leadingSpan.Length;
                    var start = MaxLeadingAffixLength - leadingLength;

                    if (leadingLength > 0)
                    {
                        leadingSpan.CopyTo(parseBuffer[start..]);
                    }

                    foreach (var trailing in TrailingAffixes)
                    {
                        var trailingSpan = trailing.AsSpan();
                        var totalLength = leadingLength + charsWritten + trailingSpan.Length;
                        Assert.IsLessThanOrEqualTo(parseBuffer.Length, totalLength);

                        trailingSpan.CopyTo(parseBuffer[(MaxLeadingAffixLength + charsWritten)..]);

                        var parseText = parseBuffer[start..(start + totalLength)];

                        var parseBCL = float.TryParse(parseText, provider: cultureInfo, out var actualBCL);
                        var parseSimd = float.TryParseSimd(parseText, provider: cultureInfo, out var actualSimd);
                        if (!(parseBCL && parseSimd))
                        {
                            Assert.Fail($"{parseText} {cultureName} {GetFormatDisplayName(format)}");
                        }

                        AssertEqualsOrNaN(v, actualBCL);
                        AssertEqualsOrNaN(v, actualSimd);
                    }
                }
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(Float32CoverageData))]
    public void SimdTest_Parse_Coverage_BCL_(ParseTextCase testCase)
    {
        var parseBCL = float.TryParse(testCase.Text, provider: testCase.CultureInfo, out var actualBCL);
        var parseSimd = float.TryParseSimd(testCase.Text, provider: testCase.CultureInfo, out var actualSimd);

        Assert.AreEqual(parseBCL, parseSimd, testCase.ToString());
        if (!parseBCL)
        {
            return;
        }

        AssertEqualsOrNaN(actualBCL, actualSimd);
        AssertEqualsOrNaN(testCase.ExpectedValue, actualSimd);
    }

    [TestMethod]
    [DynamicData(nameof(Float32TestData))]
    public void SimdTest_Parse_Float32Enumerator_RoundTripsBits(Float32TestCase testCase)
    {
        var value = testCase.Value;

        var bits = SingleToBits(value);

        Assert.AreEqual(testCase.Bits, bits);
    }

    static List<Float32TestCase> EnumerateFloat32TestData()
    {
        List<Float32TestCase> testCases = [];
        foreach (var signBit in SignBits)
        {
            for (var exponent = 0; exponent < ExponentCount; exponent++)
            {
                foreach (var mantissa in Mantissas)
                {
                    var bits = signBit | (exponent << ExponentShift) | mantissa;
                    var name = CreateName(signBit, exponent, mantissa);
                    testCases.Add(new Float32TestCase(name, bits));
                }
            }
        }
        return testCases;
    }

    static List<ParseTextCase> EnumerateFloat32CoverageData()
    {
        List<ParseTextCase> testCases = [];
        foreach (var cultureInfo in CultureInfos)
        {
            var culture = cultureInfo ?? CultureInfo.InvariantCulture;
            foreach (var value in ParseCoverageValues)
            {
                foreach (var textCase in EnumerateParseCoverageData(culture, value))
                {
                    testCases.Add(textCase);
                }
            }
        }

        return testCases;
    }

    static IEnumerable<ParseTextCase> EnumerateParseCoverageData(CultureInfo cultureInfo, float value)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var textCase in EnumerateGeneralParseCoverageData(cultureInfo, value))
        {
            if (seen.Add(textCase.Text))
            {
                yield return textCase;
            }
        }

        foreach (var textCase in EnumerateThousandsSeparatedParseCoverageData(cultureInfo, value))
        {
            if (seen.Add(textCase.Text))
            {
                yield return textCase;
            }
        }
    }

    static IEnumerable<ParseTextCase> EnumerateGeneralParseCoverageData(CultureInfo cultureInfo, float value)
    {
        foreach (var format in Formats)
        {
            var text = value.ToString(format, cultureInfo);
            foreach (var affixedText in EnumerateAffixedTexts(text, value >= 0))
            {
                yield return new ParseTextCase(
                    CreateParseCaseName(cultureInfo, format, value, "General", affixedText),
                    affixedText,
                    value,
                    cultureInfo);
            }
        }
    }

    static IEnumerable<ParseTextCase> EnumerateThousandsSeparatedParseCoverageData(CultureInfo cultureInfo, float value)
    {
        if (!float.IsFinite(value))
        {
            yield break;
        }

        var absValue = MathF.Abs(value);
        if (absValue < 1000f)
        {
            yield break;
        }

        var text = value.ToString("N6", cultureInfo);
        while (text.Contains(cultureInfo.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal) && text.EndsWith("0", StringComparison.Ordinal))
        {
            text = text[..^1];
        }

        if (text.EndsWith(cultureInfo.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal))
        {
            text = text[..^cultureInfo.NumberFormat.NumberDecimalSeparator.Length];
        }

        foreach (var affixedText in EnumerateAffixedTexts(text, value >= 0))
        {
            yield return new ParseTextCase(
                CreateParseCaseName(cultureInfo, "N6", value, "Thousands", affixedText),
                affixedText,
                value,
                cultureInfo);
        }
    }

    static IEnumerable<string> EnumerateAffixedTexts(string text, bool isPositive)
    {
        foreach (var leading in LeadingAffixes)
        {
            if (leading == "+" && !isPositive)
            {
                continue;
            }

            var prefixedText = leading == "+" ? AddLeadingPlus(text) : leading + text;
            foreach (var trailing in TrailingAffixes)
            {
                yield return prefixedText + trailing;
            }
        }
    }

    static int GetMaxLength(string[] values)
    {
        var maxLength = 0;
        foreach (var value in values)
        {
            if (value.Length > maxLength)
            {
                maxLength = value.Length;
            }
        }

        return maxLength;
    }

    static string AddLeadingPlus(string text)
        => text.Length > 0 && (text[0] == '+' || text[0] == '-') ? text : "+" + text;

    static string CreateParseCaseName(CultureInfo cultureInfo, string? format, float value, string category, string text)
        => string.Join(
            "_",
            [
                category,
                GetCultureDisplayName(cultureInfo.Name),
                GetFormatDisplayName(format),
                value.ToString("R", CultureInfo.InvariantCulture),
                text.Replace(" ", "␠", StringComparison.Ordinal).Replace("\t", "\\t", StringComparison.Ordinal)
            ]);

    static string CreateName(int signBit, int exponent, int mantissa)
    {
        var sign = signBit == 0 ? "+" : "-";
        var kind = exponent switch
        {
            0 when mantissa == 0 => "Zero",
            0 => "Subnormal",
            ExponentCount - 1 when mantissa == 0 => "Infinity",
            ExponentCount - 1 => "NaN",
            _ => "Normal"
        };

        return $"{kind}_s{sign}_e{exponent:X2}_m{mantissa:X6}";
    }

    static void AssertEqualsOrNaN(float expected, float actual)
    {
        if (float.IsNaN(expected))
        {
            Assert.IsTrue(float.IsNaN(actual));
        }
        else
        {
            Assert.AreEqual(SingleToBits(expected), SingleToBits(actual));
        }
    }

    static int SingleToBits(float value) => BitConverter.SingleToInt32Bits(value);

    static CultureInfo GetCulture(string cultureName) =>
        cultureName.Length == 0 ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName);

    static string GetCultureDisplayName(string cultureName) =>
        cultureName.Length == 0 ? "Invariant" : cultureName;

    static string GetFormatDisplayName(string? format) => format ?? "Default";

    public readonly record struct Float32TestCase(string Name, int Bits)
    {
        public float Value => BitConverter.Int32BitsToSingle(Bits);

        public override string ToString() => Name;
    }

    public readonly record struct ParseTextCase(string Name, string Text, float ExpectedValue, CultureInfo CultureInfo)
    {
        public override string ToString() => Name;
    }

}
