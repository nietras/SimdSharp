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

    static CultureInfo?[] CultureInfos { get; } = [null, new(""), new("en-US"), new("fr-FR"), new("da-DK")];

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

    public static IEnumerable<Float32TestCase> GetFloat32TestData() => EnumerateFloat32TestData();

    [TestMethod]
    [DynamicData(nameof(GetFloat32TestData))]
    public void SimdTest_Parse_RoundTrip_BCL_(Float32TestCase testCase)
    {
        var v = testCase.Value;
        Span<char> chars = stackalloc char[1024];

        foreach (var cultureInfo in CultureInfos)
        {
            foreach (var formats in Formats)
            {
                var cultureName = cultureInfo?.Name ?? "";

                Assert.IsTrue(v.TryFormat(chars, out var charsWritten, format: default, provider: cultureInfo));
                var span = chars[..charsWritten];

                var parseBCL = float.TryParse(span, provider: cultureInfo, out var actualBCL);
                var parseSimd = float.TryParseSimd(span, provider: cultureInfo, out var actualSimd);
                if (!(parseBCL && parseSimd))
                {
                    Assert.Fail($"{new string(span)} {cultureName}");
                }

                AssertEqualsOrNaN(v, actualBCL);
                AssertEqualsOrNaN(v, actualSimd);
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetFloat32TestData))]
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
                    testCases.Add(new Float32TestCase(
                        Name: CreateName(signBit, exponent, mantissa),
                        Bits: bits));
                }
            }
        }
        return testCases;
    }

    static string CreateName(int signBit, int exponent, int mantissa)
    {
        var sign = signBit == 0 ? "Positive" : "Negative";
        var kind = exponent switch
        {
            0 when mantissa == 0 => "Zero",
            0 => "Subnormal",
            ExponentCount - 1 when mantissa == 0 => "Infinity",
            ExponentCount - 1 => "NaN",
            _ => "Normal"
        };

        return $"{sign}_{kind}_E{exponent:X2}_M{mantissa:X6}";
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

    static bool IsNaN(int bits)
        => ((bits >> ExponentShift) & ExponentMask) == ExponentCount - 1 && (bits & MantissaMask) != 0;

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

}
