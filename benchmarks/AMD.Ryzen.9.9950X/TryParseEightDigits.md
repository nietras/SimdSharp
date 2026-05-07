```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.7184/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.203
  [Host]    : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v4

Job=.NET 10.0  EnvironmentVariables=DOTNET_GCDynamicAdaptationMode=0  Runtime=.NET 10.0  
Toolchain=net10.0  IterationTime=350ms  MaxIterationCount=7  
MinIterationCount=3  WarmupCount=3  

```
| Method                          | Text     | Mean      | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |--------- |----------:|------:|----------:|------------:|
| TryParseEightDigits_SimdSharp   | 12345678 | 0.1887 ns |  0.03 |         - |          NA |
| TryParseEightDigits_csFastFloat | 12345678 | 0.1985 ns |  0.03 |         - |          NA |
| TryParseEightDigits_BCL         | 12345678 | 6.0287 ns |  1.00 |         - |          NA |
