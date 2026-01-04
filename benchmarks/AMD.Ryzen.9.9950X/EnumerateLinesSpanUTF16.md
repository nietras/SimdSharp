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
| EnumerateLines_BCL           | 32768       | 0             | 101,485.7 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp     | 32768       | 0             |  14,749.3 ns |  0.15 |         - |          NA |
| EnumerateLines_New_SimdSharp | 32768       | 0             |  11,690.5 ns |  0.12 |         - |          NA |
|                              |             |               |              |       |           |             |
| EnumerateLines_BCL           | 32768       | 8             |  37,703.9 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp     | 32768       | 8             |   7,048.6 ns |  0.19 |         - |          NA |
| EnumerateLines_New_SimdSharp | 32768       | 8             |   4,923.8 ns |  0.13 |         - |          NA |
|                              |             |               |              |       |           |             |
| EnumerateLines_BCL           | 32768       | 128           |   3,536.0 ns |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp     | 32768       | 128           |     911.4 ns |  0.26 |         - |          NA |
| EnumerateLines_New_SimdSharp | 32768       | 128           |     798.0 ns |  0.23 |         - |          NA |
