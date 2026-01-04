using System;
using System.Runtime.Intrinsics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public class SimdTest_VectorPaths
{
    [TestMethod]
    public void VectorPaths_PrintHardwareAcceleration()
    {
        var v512 = Vector512.IsHardwareAccelerated;
        var v256 = Vector256.IsHardwareAccelerated;
        var v128 = Vector128.IsHardwareAccelerated;

        var activePath = v512 ? "Vector512"
                       : v256 ? "Vector256"
                       : v128 ? "Vector128"
                       : "Scalar";

        var message = $"""
            === SIMD Hardware Acceleration ===
            Vector512: {v512}
            Vector256: {v256}
            Vector128: {v128}
            Active Path: {activePath}
            ==================================
            """;

        // Use both Console and TestContext to ensure visibility
        Console.WriteLine(message);
    }
}
