# Build all Lambda Docker images for LocalStack
# This script builds Docker containers for all Lambda functions

param(
    [switch]$NoBuildCache = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Building Lambda Docker Images" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$rootDir = Split-Path -Parent $PSScriptRoot
$srcDir = Join-Path $rootDir "src"

# Lambda functions to build
$lambdaFunctions = @(
    @{ Name = "Validator"; Handler = "InvestTax.Lambda.Validator::InvestTax.Lambda.Validator.Function::FunctionHandler" },
    @{ Name = "Normalizer"; Handler = "InvestTax.Lambda.Normalizer::InvestTax.Lambda.Normalizer.Function::FunctionHandler" },
    @{ Name = "NBPClient"; Handler = "InvestTax.Lambda.NBPClient::InvestTax.Lambda.NBPClient.Function::FunctionHandler" },
    @{ Name = "Calculator"; Handler = "InvestTax.Lambda.Calculator::InvestTax.Lambda.Calculator.Function::FunctionHandler" },
    @{ Name = "ReportGenerator"; Handler = "InvestTax.Lambda.ReportGenerator::InvestTax.Lambda.ReportGenerator.Function::FunctionHandler" },
    @{ Name = "EmailSender"; Handler = "InvestTax.Lambda.EmailSender::InvestTax.Lambda.EmailSender.Function::FunctionHandler" },
    @{ Name = "Starter"; Handler = "InvestTax.Lambda.Starter::InvestTax.Lambda.Starter.Function::FunctionHandler" }
)

$buildArgs = @()
if ($NoBuildCache) {
    $buildArgs += "--no-cache"
}

$successCount = 0
$failCount = 0

foreach ($lambda in $lambdaFunctions) {
    $lambdaName = $lambda.Name
    $imageName = "investtax-lambda-$($lambdaName.ToLower())"
    
    Write-Host "Building $lambdaName..." -ForegroundColor Yellow
    Write-Host "  Image: $imageName" -ForegroundColor Gray
    
    try {
        Push-Location $srcDir
        
        $dockerFile = "InvestTax.Lambda.$lambdaName\Dockerfile"
        
        if (-not (Test-Path $dockerFile)) {
            Write-Host "  [FAIL] Dockerfile not found: $dockerFile" -ForegroundColor Red
            $failCount++
            continue
        }
        
        # Build Docker image
        docker build `
            -f $dockerFile `
            -t "${imageName}:latest" `
            -t "${imageName}:local" `
            @buildArgs `
            . 2>&1 | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  [OK] Built successfully" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "  [FAIL] Build failed" -ForegroundColor Red
            $failCount++
        }
    }
    catch {
        Write-Host "  [FAIL] Error: $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
    }
    finally {
        Pop-Location
    }
    
    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Summary" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Successful: $successCount" -ForegroundColor Green
Write-Host "  Failed:     $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Gray" })
Write-Host ""

if ($successCount -eq $lambdaFunctions.Count) {
    Write-Host "All Lambda images built successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Start LocalStack: .\scripts\start-local.ps1" -ForegroundColor White
    Write-Host "  2. Deploy Lambdas:   .\scripts\deploy-lambdas-local.ps1" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "Some builds failed. Please check the errors above." -ForegroundColor Red
    exit 1
}

# List built images
Write-Host "Built images:" -ForegroundColor Yellow
docker images | Select-String -Pattern "investtax-lambda"
Write-Host ""
