# Syrx MySQL Docker Image Build Script
# This script builds a custom MySQL Docker image with all test objects for Syrx.MySql.Tests.Integration

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "rebuild", "clean")]
    [string]$Action = "build"
)

$ErrorActionPreference = "Stop"

# Configuration
$ImageName = "docker-syrx-mysql-test"
$ImageTag = "latest"
$FullImageName = "${ImageName}:${ImageTag}"

# Ensure we're in the correct directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "Syrx MySQL Docker Image Builder" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green
Write-Host ""

switch ($Action.ToLower()) {
    "build" {
        Write-Host "Building MySQL Docker image..." -ForegroundColor Yellow
        Write-Host "Image Name: $FullImageName" -ForegroundColor Cyan
        Write-Host ""
        
        docker build -t $FullImageName .
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "MySQL Docker image built successfully!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Image Details:" -ForegroundColor Cyan
            docker images $ImageName
            Write-Host ""
            Write-Host "Usage:" -ForegroundColor Yellow
            Write-Host "  The image '$FullImageName' is now available for TestContainers" -ForegroundColor White
            Write-Host "  Run tests with: dotnet test" -ForegroundColor White
        } else {
            Write-Host "Failed to build MySQL Docker image!" -ForegroundColor Red
            exit 1
        }
    }
    
    "rebuild" {
        Write-Host "Rebuilding MySQL Docker image (no cache)..." -ForegroundColor Yellow
        Write-Host "Image Name: $FullImageName" -ForegroundColor Cyan
        Write-Host ""
        
        docker build --no-cache -t $FullImageName .
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "MySQL Docker image rebuilt successfully!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Image Details:" -ForegroundColor Cyan
            docker images $ImageName
        } else {
            Write-Host "Failed to rebuild MySQL Docker image!" -ForegroundColor Red
            exit 1
        }
    }
    
    "clean" {
        Write-Host "Cleaning up MySQL Docker image..." -ForegroundColor Yellow
        
        $imageExists = docker images -q $FullImageName
        if ($imageExists) {
            docker rmi $FullImageName
            if ($LASTEXITCODE -eq 0) {
                Write-Host "MySQL Docker image removed successfully!" -ForegroundColor Green
            } else {
                Write-Host "Failed to remove MySQL Docker image!" -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "MySQL Docker image does not exist." -ForegroundColor Yellow
        }
    }
    
    default {
        Write-Host "Invalid action: $Action" -ForegroundColor Red
        Write-Host ""
        Write-Host "Usage: .\build-mysql-image.ps1 -Action <action>" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Available actions:" -ForegroundColor Cyan
        Write-Host "  build    - Build the MySQL Docker image" -ForegroundColor White
        Write-Host "  rebuild  - Rebuild the image without using cache" -ForegroundColor White
        Write-Host "  clean    - Remove the MySQL Docker image" -ForegroundColor White
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Yellow
        Write-Host "  .\build-mysql-image.ps1 build" -ForegroundColor Gray
        Write-Host "  .\build-mysql-image.ps1 rebuild" -ForegroundColor Gray
        Write-Host "  .\build-mysql-image.ps1 clean" -ForegroundColor Gray
        exit 1
    }
}