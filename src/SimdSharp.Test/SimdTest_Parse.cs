using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public class SimdTest_Parse
{
    const int ExponentShift = 23;
    const int ExponentCount = 256;
    const int MantissaMask = 0x007F_FFFF;
    const int NegativeSignBit = unchecked((int)0x8000_0000);

    public record Float32TestCase(string Name, int Bits)
    {
        public float Value => BitConverter.Int32BitsToSingle(Bits);

        public override string ToString() => Name;
    }

    public static IEnumerable<Float32TestCase> Float32TestData { get; } = EnumerateFloat32TestData();

    [TestMethod]
    [DynamicData(nameof(Float32TestData))]
    public void SimdTest_Parse_Float32Enumerator_RoundTripsBits(Float32TestCase testCase)
    {
        var value = testCase.Value;
        var bits = BitConverter.SingleToInt32Bits(value);

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

    static ReadOnlySpan<int> SignBits => [0, NegativeSignBit];

    static ReadOnlySpan<int> Mantissas => [0, 1, MantissaMask - 1, MantissaMask];

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


}
