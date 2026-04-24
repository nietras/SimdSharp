```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.7184/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.203
  [Host]    : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v4

Job=.NET 10.0  EvaluateOverhead=False  EnvironmentVariables=DOTNET_GCDynamicAdaptationMode=0  
Runtime=.NET 10.0  Toolchain=net10.0  IterationTime=350ms  
MaxIterationCount=7  MinIterationCount=3  WarmupCount=3  

```
| Method                   | TotalLength | MaxLineLength | Mean           | Ratio | Allocated | Alloc Ratio |
|------------------------- |------------ |-------------- |---------------:|------:|----------:|------------:|
| EnumerateLines_BCL       | 143         | 0             |     419.458 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 143         | 0             |      55.128 ns |  0.13 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 143         | 8             |     173.831 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 143         | 8             |      30.703 ns |  0.18 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 143         | 128           |      15.367 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 143         | 128           |       8.197 ns |  0.53 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 32768       | 0             | 101,732.839 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 32768       | 0             |  11,697.107 ns |  0.11 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 32768       | 8             |  37,258.604 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 32768       | 8             |   5,219.503 ns |  0.14 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 32768       | 128           |   3,499.140 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 32768       | 128           |     830.216 ns |  0.24 |         - |          NA |
