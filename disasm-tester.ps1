param(
    [string]$runtime = "win-arm64"
)
dotnet publish src/SimdSharp.Tester/SimdSharp.Tester.csproj -c Release -r "$runtime" -f net10.0 --self-contained true /p:PublishAot=true /p:DebugSymbols=true
dumpbin /DISASM /SYMBOLS "artifacts\publish\SimdSharp.Tester\release_net10.0_$runtime\SimdSharp.Tester.exe" > "artifacts\publish\SimdSharp.Tester\release_net10.0_$runtime\disassembly.asm"
