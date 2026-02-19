# Start Local Development Environment
# This script starts LocalStack using Docker Compose and initializes AWS resources

param(
    [switch]$SkipInit = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  InvestTax Calculator - Local Dev Env" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Check if Docker is running
Write-Host "Checking Docker status..." -ForegroundColor Yellow
try {
    $dockerStatus = docker ps 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Docker is not running!" -ForegroundColor Red
        Write-Host "Please start Docker Desktop and try again." -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Docker is running" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Docker is not installed or not in PATH!" -ForegroundColor Red
    exit 1
}

# Start Docker Compose
Write-Host "`nStarting LocalStack environment..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start Docker Compose!" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] LocalStack containers started" -ForegroundColor Green

# Wait for LocalStack to be ready
Write-Host "`nWaiting for LocalStack to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if LocalStack is responding
$maxRetries = 6
$retryCount = 0
$localstackReady = $false

while ($retryCount -lt $maxRetries -and -not $localstackReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:4566/_localstack/health" -TimeoutSec 5 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            $localstackReady = $true
        }
    }
    catch {
        $retryCount++
        Write-Host "  Waiting... (attempt $retryCount/$maxRetries)" -ForegroundColor Gray
        Start-Sleep -Seconds 5
    }
}

if ($localstackReady) {
    Write-Host "[OK] LocalStack is ready" -ForegroundColor Green
}
else {
    Write-Host "WARNING: LocalStack may not be fully ready yet. Continuing anyway..." -ForegroundColor Yellow
}

# Initialize AWS resources
if (-not $SkipInit) {
    Write-Host "`nInitializing AWS resources..." -ForegroundColor Yellow
    
    # Check if AWS CLI is installed
    try {
        $awsInstalled = Get-Command aws -ErrorAction Stop
        & "$PSScriptRoot\..\infrastructure\localstack-init.ps1"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] AWS resources initialized" -ForegroundColor Green
        } else {
            Write-Host "WARNING: Some AWS resources may not have been initialized properly" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "WARNING: AWS CLI is not installed!" -ForegroundColor Yellow
        Write-Host "Skipping AWS resource initialization." -ForegroundColor Yellow
        Write-Host "Please install AWS CLI and run: .\infrastructure\localstack-init.ps1" -ForegroundColor Yellow
    }
}
else {
    Write-Host "`nSkipping AWS resource initialization (use -SkipInit to skip)" -ForegroundColor Gray
}

# Display summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Local Environment Ready!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Services available at:" -ForegroundColor Yellow
Write-Host "  • LocalStack Gateway:  http://localhost:4566" -ForegroundColor White
Write-Host "  • DynamoDB Admin UI:   http://localhost:8001" -ForegroundColor White
Write-Host "  • Health Check:        http://localhost:4566/_localstack/health`n" -ForegroundColor White

Write-Host "To stop the environment, run:" -ForegroundColor Yellow
Write-Host "  .\scripts\stop-local.ps1`n" -ForegroundColor White

Write-Host "To view logs, run:" -ForegroundColor Yellow
Write-Host "  docker-compose logs -f localstack`n" -ForegroundColor White
