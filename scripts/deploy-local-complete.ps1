# Complete Local Deployment for LocalStack
# This script builds and deploys the entire InvestTax application to LocalStack

param(
    [switch]$SkipBuild = $false,
    [switch]$SkipInfrastructure = $false
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  InvestTax LocalStack Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$startTime = Get-Date

# Step 1: Start LocalStack
if (-not $SkipInfrastructure) {
    Write-Host "[1/5] Starting LocalStack..." -ForegroundColor Cyan
    Write-Host ""
    
    try {
        $health = Invoke-RestMethod -Uri "http://localhost:4566/_localstack/health" -TimeoutSec 2 -ErrorAction Stop
        Write-Host "[OK] LocalStack is already running" -ForegroundColor Green
    }
    catch {
        Write-Host "Starting LocalStack containers..." -ForegroundColor Yellow
        & "$PSScriptRoot\start-local.ps1"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[FAIL] Failed to start LocalStack" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host ""
} else {
    Write-Host "[1/5] Skipping infrastructure setup" -ForegroundColor Gray
    Write-Host ""
}

# Step 2: Build Lambda Docker images
if (-not $SkipBuild) {
    Write-Host "[2/5] Building Lambda Docker images..." -ForegroundColor Cyan
    Write-Host ""
    
    & "$PSScriptRoot\build-lambda-images.ps1"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] Failed to build Lambda images" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
} else {
    Write-Host "[2/5] Skipping Lambda image build" -ForegroundColor Gray
    Write-Host ""
}

# Step 3: Deploy Lambda functions to LocalStack
Write-Host "[3/5] Deploying Lambda functions..." -ForegroundColor Cyan
Write-Host ""

& "$PSScriptRoot\deploy-lambdas-local.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Failed to deploy Lambda functions" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 4: Deploy Step Functions state machine
Write-Host "[4/5] Deploying Step Functions..." -ForegroundColor Cyan
Write-Host ""

& "$PSScriptRoot\deploy-stepfunctions-local.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Failed to deploy Step Functions" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 5: Verify deployment
Write-Host "[5/5] Verifying deployment..." -ForegroundColor Cyan
Write-Host ""

$env:AWS_ACCESS_KEY_ID = "test"
$env:AWS_SECRET_ACCESS_KEY = "test"

try {
    # Check Lambda functions
    Write-Host "Lambda functions:" -ForegroundColor Yellow
    $functions = aws --endpoint-url=http://localhost:4566 lambda list-functions --region eu-central-1 2>&1 | ConvertFrom-Json
    $investTaxFunctions = $functions.Functions | Where-Object { $_.FunctionName -like "*InvestTax*" }
    
    if ($investTaxFunctions.Count -gt 0) {
        Write-Host "  [OK] $($investTaxFunctions.Count) functions deployed" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] No functions found" -ForegroundColor Yellow
    }
    
    # Check Step Functions
    Write-Host "Step Functions:" -ForegroundColor Yellow
    $stateMachines = aws --endpoint-url=http://localhost:4566 stepfunctions list-state-machines --region eu-central-1 2>&1 | ConvertFrom-Json
    $investTaxSM = $stateMachines.stateMachines | Where-Object { $_.name -like "*InvestTax*" }
    
    if ($investTaxSM) {
        Write-Host "  [OK] State machine deployed" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] State machine not found" -ForegroundColor Yellow
    }
    
    # Check S3 buckets
    Write-Host "S3 Buckets:" -ForegroundColor Yellow
    $buckets = aws --endpoint-url=http://localhost:4566 s3 ls 2>&1
    if ($buckets -match "investtax") {
        Write-Host "  [OK] Buckets configured" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Buckets not found" -ForegroundColor Yellow
    }
    
    # Check DynamoDB
    Write-Host "DynamoDB:" -ForegroundColor Yellow
    $tables = aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-central-1 2>&1 | ConvertFrom-Json
    if ($tables.TableNames -contains "InvestTax-Jobs-Local") {
        Write-Host "  [OK] Jobs table exists" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Jobs table not found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "[WARN] Verification partially failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""

$endTime = Get-Date
$duration = $endTime - $startTime

# Final summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Duration: $($duration.Minutes)m $($duration.Seconds)s" -ForegroundColor Gray
Write-Host ""
Write-Host "Your local environment is ready!" -ForegroundColor Green
Write-Host ""
Write-Host "Quick Links:" -ForegroundColor Yellow
Write-Host "  DynamoDB Admin:  http://localhost:8001" -ForegroundColor Cyan
Write-Host "  LocalStack:      http://localhost:4566" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test your workflow:" -ForegroundColor Yellow
Write-Host "  1. Upload test file:  .\scripts\test-upload.ps1" -ForegroundColor White
Write-Host "  2. Verify results:    .\scripts\verify-workflow.ps1" -ForegroundColor White
Write-Host "  3. Check DynamoDB:    Start-Process http://localhost:8001" -ForegroundColor White
Write-Host ""
Write-Host "Example:" -ForegroundColor Yellow
Write-Host "  .\scripts\test-upload.ps1 -CsvFile 'test-data\test-1-simple-success.csv'" -ForegroundColor White
Write-Host ""
