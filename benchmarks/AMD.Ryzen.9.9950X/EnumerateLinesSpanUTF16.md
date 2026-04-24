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
| EnumerateLines_BCL       | 143         | 0             |     419.013 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 143         | 0             |      53.888 ns |  0.13 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 143         | 8             |     173.825 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 143         | 8             |      27.933 ns |  0.16 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 143         | 128           |      15.425 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 143         | 128           |       7.553 ns |  0.49 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 32768       | 0             | 101,518.412 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 32768       | 0             |  11,228.387 ns |  0.11 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 32768       | 8             |  37,248.524 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 32768       | 8             |   4,943.082 ns |  0.13 |         - |          NA |
|                          |             |               |                |       |           |             |
| EnumerateLines_BCL       | 32768       | 128           |   3,505.707 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 32768       | 128           |     765.798 ns |  0.22 |         - |          NA |
