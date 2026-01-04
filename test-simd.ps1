#!/usr/bin/env pwsh
# Script to run tests with different SIMD vector paths exercised
# Supports both x86/x64 (SSE/AVX) and ARM64 (AdvSimd) platforms

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running SIMD Tests with All Paths" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

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
Write-Host "Detected Architecture: $arch" -ForegroundColor Magenta
Write-Host ""

function Get-EnvValue($value) {
    if ($null -eq $value) { "(not set)" } else { $value }
}

function Show-EnvVars {
    Write-Host "  DOTNET_EnableHWIntrinsic    = $(Get-EnvValue $env:DOTNET_EnableHWIntrinsic)" -ForegroundColor Gray
    if ($isArm) {
        Write-Host "  DOTNET_EnableArm64AdvSimd   = $(Get-EnvValue $env:DOTNET_EnableArm64AdvSimd)" -ForegroundColor Gray
    } else {
        Write-Host "  DOTNET_EnableAVX512         = $(Get-EnvValue $env:DOTNET_EnableAVX512)" -ForegroundColor Gray
        Write-Host "  DOTNET_EnableAVX2           = $(Get-EnvValue $env:DOTNET_EnableAVX2)" -ForegroundColor Gray
        Write-Host "  DOTNET_EnableAVX            = $(Get-EnvValue $env:DOTNET_EnableAVX)" -ForegroundColor Gray
        Write-Host "  DOTNET_EnableSSE2           = $(Get-EnvValue $env:DOTNET_EnableSSE2)" -ForegroundColor Gray
    }
    Write-Host ""
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
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Test Run 1: Default (All SIMD enabled)" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

Clear-SimdEnvVars
Show-EnvVars
Run-SimdTest

Write-Host ""

if ($isArm) {
    # ARM64: Vector128 is the native width, Vector256/512 use software fallback
    # Test 2: Vector128 path (AdvSimd - native ARM SIMD)
    Write-Host "----------------------------------------" -ForegroundColor Yellow
    Write-Host "Test Run 2: Vector128 path (AdvSimd - native ARM SIMD)" -ForegroundColor Yellow
    Write-Host "----------------------------------------" -ForegroundColor Yellow

    Clear-SimdEnvVars
    Show-EnvVars
    Run-SimdTest

    Write-Host ""
} else {
    # x64/x86: Test Vector256 and Vector128 paths
    
    # Test 2: Vector256 path (disable AVX-512)
    Write-Host "----------------------------------------" -ForegroundColor Yellow
    Write-Host "Test Run 2: Vector256 path (AVX-512 disabled)" -ForegroundColor Yellow
    Write-Host "----------------------------------------" -ForegroundColor Yellow

    Clear-SimdEnvVars
    $env:DOTNET_EnableAVX512 = "0"

    Show-EnvVars
    Run-SimdTest

    Write-Host ""

    # Test 3: Vector128 path (disable AVX-512 and AVX2/AVX)
    Write-Host "----------------------------------------" -ForegroundColor Yellow
    Write-Host "Test Run 3: Vector128 path (AVX-512 and AVX disabled)" -ForegroundColor Yellow
    Write-Host "----------------------------------------" -ForegroundColor Yellow

    Clear-SimdEnvVars
    $env:DOTNET_EnableAVX512 = "0"
    $env:DOTNET_EnableAVX2 = "0"
    $env:DOTNET_EnableAVX = "0"

    Show-EnvVars
    Run-SimdTest

    Write-Host ""
}

# Test: Scalar path (all hardware intrinsics disabled) - works on all platforms
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Test Run: Scalar path (All HW intrinsics disabled)" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

Clear-SimdEnvVars
$env:DOTNET_EnableHWIntrinsic = "0"

Show-EnvVars
Run-SimdTest

Clear-SimdEnvVars
Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
if ($failed) {
    Write-Host "SIMD Tests: SOME TESTS FAILED" -ForegroundColor Red
    exit 1
} else {
    Write-Host "SIMD Tests: ALL PASSED" -ForegroundColor Green
    exit 0
}
