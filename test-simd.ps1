#!/usr/bin/env pwsh
# Script to run tests with different SIMD vector paths exercised
# Supports both x86/x64 (SSE/AVX) and ARM64 (AdvSimd) platforms

[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '')]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '')]
param()

$ErrorActionPreference = "Stop"

Write-Output "========================================"
Write-Output "Running SIMD Tests with All Paths"
Write-Output "========================================"
Write-Output ""

$failed = $false

# Detect platform - compatible with PowerShell 5.1+
$arch = $env:PROCESSOR_ARCHITECTURE
if (-not $arch) {
    # Fallback for non-Windows or if env var not set
    try {
        $arch = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
    } catch {
        $arch = "Unknown"
    }
}
$isArm = $arch -match "ARM"
Write-Output "Detected Architecture: $arch"
Write-Output ""

function Get-EnvValue($value) {
    if ($null -eq $value) { "(not set)" } else { $value }
}

function Show-EnvVars {
    Write-Output "  DOTNET_EnableHWIntrinsic    = $(Get-EnvValue $env:DOTNET_EnableHWIntrinsic)"
    if ($isArm) {
        Write-Output "  DOTNET_EnableArm64AdvSimd   = $(Get-EnvValue $env:DOTNET_EnableArm64AdvSimd)"
    } else {
        Write-Output "  DOTNET_EnableAVX512         = $(Get-EnvValue $env:DOTNET_EnableAVX512)"
        Write-Output "  DOTNET_EnableAVX2           = $(Get-EnvValue $env:DOTNET_EnableAVX2)"
        Write-Output "  DOTNET_EnableAVX            = $(Get-EnvValue $env:DOTNET_EnableAVX)"
        Write-Output "  DOTNET_EnableSSE2           = $(Get-EnvValue $env:DOTNET_EnableSSE2)"
    }
    Write-Output ""
}

function Clear-SimdEnvVars {
    $env:DOTNET_EnableHWIntrinsic = $null
    # x64/x86
    $env:DOTNET_EnableAVX512 = $null
    $env:DOTNET_EnableAVX2 = $null
    $env:DOTNET_EnableAVX = $null
    $env:DOTNET_EnableSSE2 = $null
    # ARM64
    $env:DOTNET_EnableArm64AdvSimd = $null
}

function Run-SimdTest {
    dotnet test --no-build -c Release --logger "console;verbosity=detailed"
    if ($LASTEXITCODE -ne 0) { $script:failed = $true }
}

dotnet build -c Release

# Test 1: Default (all SIMD enabled - uses highest available)
Write-Output "----------------------------------------"
Write-Output "Test Run 1: Default (All SIMD enabled)"
Write-Output "----------------------------------------"

Clear-SimdEnvVars
Show-EnvVars
Run-SimdTest

Write-Output ""

if ($isArm) {
    # ARM64: Vector128 is the native width, Vector256/512 use software fallback
    # Test 2: Vector128 path (AdvSimd - native ARM SIMD)
    Write-Output "----------------------------------------"
    Write-Output "Test Run 2: Vector128 path (AdvSimd - native ARM SIMD)"
    Write-Output "----------------------------------------"

    Clear-SimdEnvVars
    Show-EnvVars
    Run-SimdTest

    Write-Output ""
} else {
    # x64/x86: Test Vector256 and Vector128 paths

    # Test 2: Vector256 path (disable AVX-512)
    Write-Output "----------------------------------------"
    Write-Output "Test Run 2: Vector256 path (AVX-512 disabled)"
    Write-Output "----------------------------------------"

    Clear-SimdEnvVars
    $env:DOTNET_EnableAVX512 = "0"

    Show-EnvVars
    Run-SimdTest

    Write-Output ""

    # Test 3: Vector128 path (disable AVX-512 and AVX2/AVX)
    Write-Output "----------------------------------------"
    Write-Output "Test Run 3: Vector128 path (AVX-512 and AVX disabled)"
    Write-Output "----------------------------------------"

    Clear-SimdEnvVars
    $env:DOTNET_EnableAVX512 = "0"
    $env:DOTNET_EnableAVX2 = "0"
    $env:DOTNET_EnableAVX = "0"

    Show-EnvVars
    Run-SimdTest

    Write-Output ""
}

# Test: Scalar path (all hardware intrinsics disabled) - works on all platforms
Write-Output "----------------------------------------"
Write-Output "Test Run: Scalar path (All HW intrinsics disabled)"
Write-Output "----------------------------------------"

Clear-SimdEnvVars
$env:DOTNET_EnableHWIntrinsic = "0"

Show-EnvVars
Run-SimdTest

Clear-SimdEnvVars
Write-Output ""

# Summary
Write-Output "========================================"
if ($failed) {
    Write-Output "SIMD Tests: SOME TESTS FAILED"
    exit 1
} else {
    Write-Output "SIMD Tests: ALL PASSED"
    exit 0
}
