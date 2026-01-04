using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public class SimdTest_VectorPaths
{
    [TestMethod]
    public void SimdTest_VectorPaths_PrintHardwareAcceleration()
    {
        // Hardware intrinsics (actual CPU instruction sets - controlled by DOTNET_Enable* env vars)
        var avx512f = Avx512F.IsSupported;
        var avx2 = Avx2.IsSupported;
        var sse2 = Sse2.IsSupported;
        var advSimd = AdvSimd.IsSupported;
        var advSimd64 = AdvSimd.Arm64.IsSupported;

        // Vector types (may use software fallback even when intrinsics disabled)
        var v512 = Vector512.IsHardwareAccelerated;
        var v256 = Vector256.IsHardwareAccelerated;
        var v128 = Vector128.IsHardwareAccelerated;

        var message =
            $"""
                === SIMD Hardware Intrinsics (CPU instruction sets) ===
                Avx512F.IsSupported:             {avx512f}
                Avx2.IsSupported:                {avx2}
                Sse2.IsSupported:                {sse2}
                AdvSimd.IsSupported:             {advSimd}
                AdvSimd.Arm64.IsSupported:       {advSimd64}
                Vector512.IsHardwareAccelerated: {v512}
                Vector256.IsHardwareAccelerated: {v256}
                Vector128.IsHardwareAccelerated: {v128}
                ========================================================
            """;

        Console.Write(message);
    }
}
