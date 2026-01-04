#!/usr/bin/env pwsh
# Script to run tests with different SIMD vector paths exercised

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running SIMD Tests with All Paths" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$testProject = "src\SimdSharp.Test\SimdSharp.Test.csproj"
$failed = $false

function Get-EnvValue($value) {
    if ($null -eq $value) { "(not set)" } else { $value }
}

function Show-EnvVars {
    Write-Host "  DOTNET_EnableHWIntrinsic = $(Get-EnvValue $env:DOTNET_EnableHWIntrinsic)" -ForegroundColor Gray
    Write-Host "  DOTNET_EnableAVX512F     = $(Get-EnvValue $env:DOTNET_EnableAVX512F)" -ForegroundColor Gray
    Write-Host "  DOTNET_EnableAVX2        = $(Get-EnvValue $env:DOTNET_EnableAVX2)" -ForegroundColor Gray
    Write-Host "  DOTNET_EnableAVX         = $(Get-EnvValue $env:DOTNET_EnableAVX)" -ForegroundColor Gray
    Write-Host "  DOTNET_EnableSSE2        = $(Get-EnvValue $env:DOTNET_EnableSSE2)" -ForegroundColor Gray
    Write-Host ""
}

# Test 1: Default (all SIMD enabled - uses highest available: Vector512 > Vector256 > Vector128)
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Test Run 1: Default (All SIMD enabled)" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

# Clear any previously set environment variables
$env:DOTNET_EnableHWIntrinsic = $null
$env:DOTNET_EnableAVX512F = $null
$env:DOTNET_EnableAVX2 = $null
$env:DOTNET_EnableAVX = $null
$env:DOTNET_EnableSSE2 = $null

Show-EnvVars

dotnet test $testProject --no-build -c Release
if ($LASTEXITCODE -ne 0) { $failed = $true }

Write-Host ""

# Test 2: Vector256 path (disable AVX-512)
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Test Run 2: Vector256 path (AVX-512 disabled)" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

$env:DOTNET_EnableAVX512F = "0"

Show-EnvVars

dotnet test $testProject --no-build -c Release
if ($LASTEXITCODE -ne 0) { $failed = $true }

$env:DOTNET_EnableAVX512F = $null
Write-Host ""

# Test 3: Vector128 path (disable AVX-512 and AVX2/AVX)
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Test Run 3: Vector128 path (AVX-512 and AVX disabled)" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

$env:DOTNET_EnableAVX512F = "0"
$env:DOTNET_EnableAVX2 = "0"
$env:DOTNET_EnableAVX = "0"

Show-EnvVars

dotnet test $testProject --no-build -c Release
if ($LASTEXITCODE -ne 0) { $failed = $true }

$env:DOTNET_EnableAVX512F = $null
$env:DOTNET_EnableAVX2 = $null
$env:DOTNET_EnableAVX = $null
Write-Host ""

# Test 4: Scalar path (all hardware intrinsics disabled)
Write-Host "----------------------------------------" -ForegroundColor Yellow
Write-Host "Test Run 4: Scalar path (All HW intrinsics disabled)" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

$env:DOTNET_EnableHWIntrinsic = "0"

Show-EnvVars

dotnet test $testProject --no-build -c Release
if ($LASTEXITCODE -ne 0) { $failed = $true }

$env:DOTNET_EnableHWIntrinsic = $null
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
