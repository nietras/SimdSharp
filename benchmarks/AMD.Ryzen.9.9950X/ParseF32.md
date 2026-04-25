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
| Method               | Text        | Mean      | Ratio | Allocated | Alloc Ratio |
|--------------------- |------------ |----------:|------:|----------:|------------:|
| ParseF32_SimdSharp   | 1234567.890 | 25.936 ns |  1.03 |         - |          NA |
| ParseF32_csFastFloat | 1234567.890 |  9.022 ns |  0.36 |         - |          NA |
| ParseF32_BCL         | 1234567.890 | 25.297 ns |  1.00 |         - |          NA |
