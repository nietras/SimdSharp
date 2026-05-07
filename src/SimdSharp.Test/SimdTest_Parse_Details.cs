using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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

    [TestMethod]
    public void SimdTest_Parse_Details_TryParseEightDigits_Avx512Test()
    {
        var text = "+-0,123,456.789eE-21";
        fixed (char* chars = text)
        {
            var v = LoadLessThanLengthIndicis(chars, text.Length);
            var a = AllValid(v, text.Length);
            Assert.IsTrue(a);
        }
    }

    [TestMethod]
    public void SimdTest_Parse_Details_AllValid_AllSupportedCharacters_WorkAtShortLength()
    {
        const string validChars = "0123456789+-,.Ee";
        foreach (var c in validChars)
        {
            var sut = new string(c, 7);
            Assert.IsTrue(AllValid(CreateByteVector(sut), sut.Length), $"Character '{c}' should be valid.");
        }
    }

    [TestMethod]
    public void SimdTest_Parse_Details_AllValid_AllUnsupportedCharacters_ReturnFalse()
    {
        for (var i = 0; i <= 0x7F; i++)
        {
            var c = (char)i;
            if ("0123456789+-,.Ee".Contains(c))
            {
                continue;
            }

            var sut = new string(c, 7);
            Assert.IsFalse(AllValid(CreateByteVector(sut), sut.Length), $"Character '{c}' should be invalid.");
        }
    }

    [TestMethod]
    public void SimdTest_Parse_Details_AllValid_InvalidActiveCharacter_ReturnsFalse()
    {
        const string sut = "123:567";
        Assert.IsFalse(AllValid(CreateByteVector(sut), sut.Length));
    }

    [TestMethod]
    public void SimdTest_Parse_Details_AllValid_IgnoresInactiveTail()
    {
        const string sut = "1234567:::::::::::::::::::::::::";
        Assert.IsTrue(AllValid(CreateByteVector(sut), 7));
    }

    static Vector256<byte> CreateByteVector(string text)
    {
        Span<byte> bytes = stackalloc byte[Vector256<byte>.Count];
        var copyLength = Math.Min(text.Length, bytes.Length);
        for (var i = 0; i < copyLength; i++)
        {
            bytes[i] = checked((byte)text[i]);
        }

        return Unsafe.ReadUnaligned<Vector256<byte>>(ref MemoryMarshal.GetReference(bytes));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static Vector256<byte> LoadLessThanLengthIndicis(char* chars, nint length)
    {
        Debug.Assert(length <= Vector512<ushort>.Count);
        if (length <= Vector512<ushort>.Count)
        {
            var mask = Vector512.LessThan(Vector512<ushort>.Indices, Vector512.Create((ushort)length)).AsByte();
            var v = Avx512BW.MaskLoad((byte*)chars, mask, Vector512<byte>.Zero).AsUInt16();
            var packed = Vector512.NarrowWithSaturation(v, v);
            _ = packed;
            var lower = Vector512.GetLower(packed);
            return lower;
        }
        else
        {
            // Slow path
            return default;
        }
    }

    static readonly Vector256<byte> HighNibbleLut =
        Vector256.Create(
            (byte)0x00, // 0
            0x00,       // 1
            0x2E,       // 2 => + , - .
            0x01,       // 3 => digits
            0x10,       // 4 => E
            0x00,       // 5
            0x10,       // 6 => e
            0x00,       // 7
            0x00,       // 8
            0x00,       // 9
            0x00,       // A
            0x00,       // B
            0x00,       // C
            0x00,       // D
            0x00,       // E
            0x00,       // F,

            // repeated for upper lane
            0x00, 0x00, 0x2E, 0x01, 0x10, 0x00, 0x10, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);

    static readonly Vector256<byte> LowNibbleLut =
        Vector256.Create(
            (byte)0x01, // 0
            0x01,       // 1
            0x01,       // 2
            0x01,       // 3
            0x01,       // 4
            0x11,       // 5 => digit or exponent
            0x01,       // 6
            0x01,       // 7
            0x01,       // 8
            0x01,       // 9
            0x00,       // A
            0x02,       // B => +
            0x20,       // C => comma
            0x04,       // D => -
            0x08,       // E => .
            0x00,       // F

            // repeated
            0x01, 0x01, 0x01, 0x01, 0x01, 0x11, 0x01, 0x01,
            0x01, 0x01, 0x00, 0x02, 0x20, 0x04, 0x08, 0x00);

    public static bool AllValid(Vector256<byte> bytes, nint length)
    {
        Debug.Assert(length >= 0 && length <= Vector256<byte>.Count);
        var activeMask =
            length >= Vector256<byte>.Count
                ? -1
                : (1 << (int)length) - 1;

        // ASCII reject
        //
        // movemask extracts sign bits
        if ((Avx2.MoveMask(bytes) & activeMask) != 0)
            return false;

        var lowNibble =
            Avx2.And(bytes, Vector256.Create((byte)0x0F));

        var highNibble16 =
            Avx2.ShiftRightLogical(bytes.AsUInt16(), 4);

        var highNibble =
            Avx2.And(
                highNibble16.AsByte(),
                Vector256.Create((byte)0x0F));

        // nibble LUT lookup
        var highClass =
            Avx2.Shuffle(HighNibbleLut, highNibble);

        var lowClass =
            Avx2.Shuffle(LowNibbleLut, lowNibble);

        // intersection nonzero => valid
        var valid =
            Avx2.And(highClass, lowClass);

        // valid == 0 => invalid
        var zero =
            Vector256<byte>.Zero;

        var invalid =
            Avx2.CompareEqual(valid, zero);
        var mask = Avx2.MoveMask(invalid) & activeMask;
        return mask == 0;
    }
}
