```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6691/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v4

Job=.NET 10.0  EvaluateOverhead=False  EnvironmentVariables=DOTNET_GCDynamicAdaptationMode=0  
Runtime=.NET 10.0  Toolchain=net10.0  IterationTime=350ms  
MaxIterationCount=7  MinIterationCount=3  WarmupCount=3  

```
| Method                       | TotalLength | MaxLineLength | Mean         | Ratio | Allocated | Alloc Ratio |
|----------------------------- |------------ |-------------- |-------------:|------:|----------:|------------:|
| EnumerateLines_BCL           | 32768       | 0             | 101,853.8 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp     | 32768       | 0             |  14,426.3 ns |  0.14 |         - |          NA |
| EnumerateLines_New_SimdSharp | 32768       | 0             |  17,910.6 ns |  0.18 |         - |          NA |
|                              |             |               |              |       |           |             |
| EnumerateLines_BCL           | 32768       | 8             |  37,175.4 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp     | 32768       | 8             |   6,886.1 ns |  0.19 |         - |          NA |
| EnumerateLines_New_SimdSharp | 32768       | 8             |   9,431.1 ns |  0.25 |         - |          NA |
|                              |             |               |              |       |           |             |
| EnumerateLines_BCL           | 32768       | 128           |   3,497.7 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp     | 32768       | 128           |     969.2 ns |  0.28 |         - |          NA |
| EnumerateLines_New_SimdSharp | 32768       | 128           |   1,837.7 ns |  0.53 |         - |          NA |
