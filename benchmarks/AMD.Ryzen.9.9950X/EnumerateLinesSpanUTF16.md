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
| EnumerateLines_BCL       | 0             | 3,300.44 μs |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 0             |   533.96 μs |  0.16 |         - |          NA |
|                          |               |             |       |           |             |
| EnumerateLines_BCL       | 8             | 1,219.85 μs |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 8             |   315.16 μs |  0.26 |         - |          NA |
|                          |               |             |       |           |             |
| EnumerateLines_BCL       | 128           |   112.75 μs |  1.00 |         - |          NA |
| EnumerateLines_SimdSharp | 128           |    32.53 μs |  0.29 |         - |          NA |
