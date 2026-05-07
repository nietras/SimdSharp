using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public unsafe class SimdTest_Parse_Details
{
    [DataRow(true, "12345678")]
    [DataRow(true, "123456789")]
    [DataRow(false, "1")]
    [DataRow(false, "12")]
    [DataRow(false, "123")]
    [DataRow(false, "1234")]
    [DataRow(false, "12345")]
    [DataRow(false, "123456")]
    [DataRow(false, "1234567")]
    [DataRow(false, "/1234567")]
    [DataRow(false, ":1234567")]
    [DataRow(false, "1:234567")]
    [DataRow(false, "1234567:")]
    [DataRow(false, "1234567 ")]
    [DataRow(false, "1234 567")]
    [TestMethod]
    public void SimdTest_Parse_Details_TryParseEightDigits_ManualCases(bool shouldPass, string sut)
    {
        ref var r = ref MemoryMarshal.GetReference(sut.AsSpan());
        Assert.AreEqual(shouldPass, Simd.TryParseEightDigits(ref r, out var res));
        if (shouldPass) { Assert.AreEqual(uint.Parse(sut.AsSpan()[..8]), res); }
    }

    [TestMethod]
    public void SimdTest_Parse_Details_TryParseEightDigits_RandomCases()
    {
        var random = new Random();
        for (var i = 0; i != 850000; i++)
        {
            var RandomNumber = random.Next(10000000, 99999999);
            var sut = RandomNumber.ToString();
            ref var r = ref MemoryMarshal.GetReference(sut.AsSpan());
            Assert.IsTrue(Simd.TryParseEightDigits(ref r, out var res));
            Assert.AreEqual(uint.Parse(sut), res);
        }
    }

    [TestMethod]
    public void SimdTest_Parse_Details_TryParseEightDigits_SameEightDigits()
    {
        for (var i = 0; i <= 9; i++)
        {
            var sut = new string(i.ToString()[0], 8);
            ref var r = ref MemoryMarshal.GetReference(sut.AsSpan());
            Assert.IsTrue(Simd.TryParseEightDigits(ref r, out var res));
            Assert.AreEqual(uint.Parse(sut), res);
        }
    }
}
