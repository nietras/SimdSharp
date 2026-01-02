# SimdSharp
![.NET](https://img.shields.io/badge/net10.0-5C2D91?logo=.NET&labelColor=gray)
![C#](https://img.shields.io/badge/C%23-14.0-239120?labelColor=gray)
[![Build Status](https://github.com/nietras/SimdSharp/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/nietras/SimdSharp/actions/workflows/dotnet.yml)
[![Super-Linter](https://github.com/nietras/SimdSharp/actions/workflows/super-linter.yml/badge.svg)](https://github.com/marketplace/actions/super-linter)
[![codecov](https://codecov.io/gh/nietras/SimdSharp/branch/main/graph/badge.svg?token=WN56CR3X0D)](https://codecov.io/gh/nietras/SimdSharp)
[![CodeQL](https://github.com/nietras/SimdSharp/workflows/CodeQL/badge.svg)](https://github.com/nietras/SimdSharp/actions?query=workflow%3ACodeQL)
[![Nuget](https://img.shields.io/nuget/v/SimdSharp?color=purple)](https://www.nuget.org/packages/SimdSharp/)
[![Release](https://img.shields.io/github/v/release/nietras/SimdSharp)](https://github.com/nietras/SimdSharp/releases/)
[![downloads](https://img.shields.io/nuget/dt/SimdSharp)](https://www.nuget.org/packages/SimdSharp)
![Size](https://img.shields.io/github/repo-size/nietras/SimdSharp.svg)
[![License](https://img.shields.io/github/license/nietras/SimdSharp)](https://github.com/nietras/SimdSharp/blob/main/LICENSE)
[![Blog](https://img.shields.io/badge/blog-nietras.com-4993DD)](https://nietras.com)
![GitHub Repo stars](https://img.shields.io/github/stars/nietras/SimdSharp?style=flat)

Low-level fast SIMD algorithms in C#. Cross-platform, trimmable and
AOT/NativeAOT compatible.

⭐ Please star this project if you like it. ⭐

[Example](#example) | [Example Catalogue](#example-catalogue) | [Public API Reference](#public-api-reference)

## Example
```csharp
Simd.Empty();

// Above example code is for demonstration purposes only.
// Short names and repeated constants are only for demonstration.
```

For more examples see [Example Catalogue](#example-catalogue).

## Benchmarks
Benchmarks.

### Detailed Benchmarks

#### Comparison Benchmarks

##### Test Benchmark Results
Test.

###### AMD.Ryzen.9.9950X - Test Benchmark Results (SimdSharp 0.0.2.0, System 10.0.125.57005)

| Method          | Scope | Count | Mean   | Ratio | Allocated | Alloc Ratio |
|---------------- |------ |------ |-------:|------:|----------:|------------:|
| SimdSharp______ | Test  | 25000 | 0.0 ns |     ? |         - |           ? |


## Example Catalogue
The following examples are available in [ReadMeTest.cs](src/SimdSharp.XyzTest/ReadMeTest.cs).

### Example - Empty
```csharp
Simd.Empty();

// Above example code is for demonstration purposes only.
// Short names and repeated constants are only for demonstration.
```

## Public API Reference
```csharp
[assembly: System.CLSCompliant(false)]
[assembly: System.Reflection.AssemblyMetadata("IsAotCompatible", "True")]
[assembly: System.Reflection.AssemblyMetadata("IsTrimmable", "True")]
[assembly: System.Reflection.AssemblyMetadata("RepositoryUrl", "https://github.com/nietras/SimdSharp/")]
[assembly: System.Resources.NeutralResourcesLanguage("en")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SimdSharp.Benchmarks")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SimdSharp.ComparisonBenchmarks")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SimdSharp.Test")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SimdSharp.XyzTest")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v10.0", FrameworkDisplayName=".NET 10.0")]
namespace SimdSharp
{
    public static class Simd
    {
        public static SimdSharp.Simd.MaskSpanLineEnumeratorUTF16 EnumerateLines(System.ReadOnlySpan<char> span) { }
        public ref struct MaskSpanLineEnumeratorUTF16 : System.Collections.Generic.IEnumerator<System.ReadOnlySpan<char>>, System.Collections.IEnumerator, System.IDisposable
        {
            public System.ReadOnlySpan<char> Current { get; }
            public SimdSharp.Simd.MaskSpanLineEnumeratorUTF16 GetEnumerator() { }
            public bool MoveNext() { }
        }
    }
}
```
