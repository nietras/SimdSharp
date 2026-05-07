using System.Runtime.CompilerServices;

namespace SimdSharp.ComparisonBenchmarks;

public static unsafe class FastFloatAccessor
{
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "TryParseEightConsecutiveDigits_SIMD")]
    public static extern bool TryParseEightConsecutiveDigits_SIMD(
        // Specifies the target type as "Namespace.Class, AssemblyName"
        [UnsafeAccessorType("csFastFloat.Utils, csFastFloat")] object? targetType,
        char* start,
        out uint value
    );
}
