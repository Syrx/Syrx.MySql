# Syrx MySQL Docker Management Script
# This script helps manage the MySQL Docker container for integration testing

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("start", "stop", "restart", "status", "logs", "clean", "test", "connect")]
    [string]$Action = "start"
)

$ErrorActionPreference = "Stop"

# Configuration
$ComposeFile = "docker-compose.yml"
$ContainerName = "syrx-mysql-tests"
$ServiceName = "mysql"

# Ensure we're in the correct directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "Syrx MySQL Docker Management" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host ""

switch ($Action.ToLower()) {
    "start" {
        Write-Host "Starting MySQL container..." -ForegroundColor Yellow
        docker-compose up -d
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "MySQL container started successfully!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Connection Details:" -ForegroundColor Cyan
            Write-Host "  Host: localhost" -ForegroundColor White
            Write-Host "  Port: 3306" -ForegroundColor White
            Write-Host "  Database: syrx" -ForegroundColor White
            Write-Host "  Username: syrx_user" -ForegroundColor White
            Write-Host "  Password: YourStrong!Passw0rd" -ForegroundColor White
            Write-Host ""
            Write-Host "Waiting for database to be ready..." -ForegroundColor Yellow
            
            # Wait for health check to pass
            $maxAttempts = 30
            $attempt = 0
            do {
                Start-Sleep -Seconds 2
                $attempt++
                $health = docker inspect --format='{{.State.Health.Status}}' $ContainerName 2>$null
                Write-Host "." -NoNewline -ForegroundColor Yellow
            } while ($health -ne "healthy" -and $attempt -lt $maxAttempts)
            
            Write-Host ""
            if ($health -eq "healthy") {
                Write-Host "Database is ready for connections!" -ForegroundColor Green
            } else {
                Write-Host "Warning: Database health check timeout. Check logs with: .\manage-mysql.ps1 logs" -ForegroundColor Red
            }
        } else {
            Write-Host "Failed to start MySQL container!" -ForegroundColor Red
            exit 1
        }
    }
    
    "stop" {
        Write-Host "Stopping MySQL container..." -ForegroundColor Yellow
        docker-compose stop
        if ($LASTEXITCODE -eq 0) {
            Write-Host "MySQL container stopped successfully!" -ForegroundColor Green
        } else {
            Write-Host "Failed to stop MySQL container!" -ForegroundColor Red
            exit 1
        }
    }
    
    "restart" {
        Write-Host "Restarting MySQL container..." -ForegroundColor Yellow
        docker-compose restart
        if ($LASTEXITCODE -eq 0) {
            Write-Host "MySQL container restarted successfully!" -ForegroundColor Green
        } else {
            Write-Host "Failed to restart MySQL container!" -ForegroundColor Red
            exit 1
        }
    }
    
    "status" {
        Write-Host "MySQL container status:" -ForegroundColor Yellow
        docker-compose ps
        Write-Host ""
        
        $containerId = docker ps -q -f name=$ContainerName
        if ($containerId) {
            Write-Host "Container Health Status:" -ForegroundColor Cyan
            $health = docker inspect --format='{{.State.Health.Status}}' $ContainerName
            $status = docker inspect --format='{{.State.Status}}' $ContainerName
            Write-Host "  Status: $status" -ForegroundColor White
            Write-Host "  Health: $health" -ForegroundColor White
        } else {
            Write-Host "Container is not running." -ForegroundColor Red
        }
    }
    
    "logs" {
        Write-Host "MySQL container logs:" -ForegroundColor Yellow
        docker-compose logs $ServiceName
    }
    
    "clean" {
        Write-Host "Cleaning up MySQL environment..." -ForegroundColor Yellow
        Write-Host "This will remove containers, volumes, and networks." -ForegroundColor Red
        $confirm = Read-Host "Are you sure? (y/N)"
        
        if ($confirm -eq "y" -or $confirm -eq "Y") {
            docker-compose down -v --remove-orphans
            if ($LASTEXITCODE -eq 0) {
                Write-Host "MySQL environment cleaned successfully!" -ForegroundColor Green
            } else {
                Write-Host "Failed to clean MySQL environment!" -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "Clean operation cancelled." -ForegroundColor Yellow
        }
    }
    
    "test" {
        Write-Host "Running integration tests..." -ForegroundColor Yellow
        
        # Check if container is running
        $containerId = docker ps -q -f name=$ContainerName
        if (-not $containerId) {
            Write-Host "MySQL container is not running. Starting it first..." -ForegroundColor Yellow
            & $MyInvocation.MyCommand.Path -Action start
        }
        
        # Navigate to test project directory
        $testProjectDir = Split-Path -Parent $ScriptDir
        Set-Location $testProjectDir
        
        Write-Host "Running Syrx MySQL integration tests..." -ForegroundColor Cyan
        dotnet test --verbosity normal
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Integration tests completed successfully!" -ForegroundColor Green
        } else {
            Write-Host "Integration tests failed!" -ForegroundColor Red
            exit 1
        }
    }
    
    "connect" {
        Write-Host "Connecting to MySQL database..." -ForegroundColor Yellow
        
        # Check if container is running
        $containerId = docker ps -q -f name=$ContainerName
        if (-not $containerId) {
            Write-Host "MySQL container is not running. Please start it first with: .\manage-mysql.ps1 start" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "Opening mysql connection to Syrx database..." -ForegroundColor Cyan
        Write-Host "Use SHOW TABLES; to list tables, exit to quit" -ForegroundColor Yellow
        docker exec -it $ContainerName mysql -u syrx_user -pYourStrong!Passw0rd syrx
    }
    
    default {
        Write-Host "Invalid action: $Action" -ForegroundColor Red
        Write-Host ""
        Write-Host "Usage: .\manage-mysql.ps1 -Action <action>" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Available actions:" -ForegroundColor Cyan
        Write-Host "  start    - Start the MySQL container" -ForegroundColor White
        Write-Host "  stop     - Stop the MySQL container" -ForegroundColor White
        Write-Host "  restart  - Restart the MySQL container" -ForegroundColor White
        Write-Host "  status   - Show container status and health" -ForegroundColor White
        Write-Host "  logs     - Show container logs" -ForegroundColor White
        Write-Host "  clean    - Remove containers, volumes, and networks" -ForegroundColor White
        Write-Host "  test     - Run integration tests" -ForegroundColor White
        Write-Host "  connect  - Connect to database with mysql client" -ForegroundColor White
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Yellow
        Write-Host "  .\manage-mysql.ps1 start" -ForegroundColor Gray
        Write-Host "  .\manage-mysql.ps1 test" -ForegroundColor Gray
        Write-Host "  .\manage-mysql.ps1 logs" -ForegroundColor Gray
        exit 1
    }
}