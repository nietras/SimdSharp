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
| Method                   | MaxLineLength | Mean        | Ratio | Allocated | Alloc Ratio |
|------------------------- |-------------- |------------:|------:|----------:|------------:|
| EnumerateLines_BCL       | 0             | 3,319.38 μs |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 0             |   499.39 μs |  0.15 |         - |          NA |
|                          |               |             |       |           |             |
| EnumerateLines_BCL       | 8             | 1,229.54 μs |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 8             |   313.22 μs |  0.25 |         - |          NA |
|                          |               |             |       |           |             |
| EnumerateLines_BCL       | 128           |   112.96 μs |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 128           |    29.63 μs |  0.26 |         - |          NA |
