# Build and Deploy Lambda Functions for InvestTax Calculator
# This script builds all Lambda functions and deploys the infrastructure using CDK

param(
    [string]$Stage = "dev",
    [switch]$SkipBuild,
    [switch]$SkipTests,
    [switch]$LocalOnly
)

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "InvestTax Calculator Deployment" -ForegroundColor Cyan
Write-Host "Stage: $Stage" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

# Navigate to root directory
Set-Location $RootDir

# Step 1: Build Lambda Functions
if (-not $SkipBuild) {
    Write-Host "[1/5] Building Lambda Functions..." -ForegroundColor Yellow
    
    $lambdaProjects = @(
        "InvestTax.Lambda.Validator",
        "InvestTax.Lambda.Normalizer",
        "InvestTax.Lambda.NBPClient",
        "InvestTax.Lambda.Calculator",
        "InvestTax.Lambda.ReportGenerator",
        "InvestTax.Lambda.EmailSender",
        "InvestTax.Lambda.Starter"
    )
    
    foreach ($project in $lambdaProjects) {
        Write-Host "  Building $project..." -ForegroundColor Gray
        $projectPath = Join-Path $RootDir "src\$project\$project.csproj"
        
        dotnet publish $projectPath `
            --configuration Release `
            --runtime linux-x64 `
            --self-contained false `
            --output "src\$project\bin\Release\net10.0\publish"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ✗ Failed to build $project" -ForegroundColor Red
            exit 1
        }
        Write-Host "  ✓ Built $project" -ForegroundColor Green
    }
    Write-Host ""
} else {
    Write-Host "[1/5] Skipping build (using existing binaries)" -ForegroundColor Yellow
    Write-Host ""
}

# Step 2: Run Tests
if (-not $SkipTests) {
    Write-Host "[2/5] Running Tests..." -ForegroundColor Yellow
    
    dotnet test --configuration Release --no-build --verbosity minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Tests failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✓ All tests passed" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[2/5] Skipping tests" -ForegroundColor Yellow
    Write-Host ""
}

# Step 3: Build CDK
Write-Host "[3/5] Building CDK Stack..." -ForegroundColor Yellow
Set-Location "$RootDir\infrastructure\cdk"

npm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ NPM install failed" -ForegroundColor Red
    exit 1
}

npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ CDK build failed" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ CDK stack built successfully" -ForegroundColor Green
Write-Host ""

# Step 4: Synthesize CloudFormation
Write-Host "[4/5] Synthesizing CloudFormation Template..." -ForegroundColor Yellow
cdk synth InvestTaxStack-$Stage
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ CDK synth failed" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ CloudFormation template synthesized" -ForegroundColor Green
Write-Host ""

# Step 5: Deploy (if not local-only)
if ($LocalOnly) {
    Write-Host "[5/5] Skipping deployment (local-only mode)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "✓ Build completed successfully" -ForegroundColor Green
    Write-Host "  To deploy to AWS, run: cdk deploy InvestTaxStack-$Stage" -ForegroundColor Gray
} else {
    Write-Host "[5/5] Deploying to AWS..." -ForegroundColor Yellow
    Write-Host "  This will deploy the stack to stage: $Stage" -ForegroundColor Gray
    Write-Host "  Press Ctrl+C to cancel, or wait 5 seconds to continue..." -ForegroundColor Gray
    Start-Sleep -Seconds 5
    
    cdk deploy InvestTaxStack-$Stage --require-approval never
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Deployment failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✓ Deployment completed successfully" -ForegroundColor Green
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Stage: $Stage" -ForegroundColor White
Write-Host "Upload Bucket: investtax-upload-$Stage" -ForegroundColor White
Write-Host "Processing Bucket: investtax-processing-$Stage" -ForegroundColor White
Write-Host "Jobs Table: InvestTax-Jobs-$Stage" -ForegroundColor White
Write-Host "State Machine: InvestTax-Workflow-$Stage" -ForegroundColor White
Write-Host ""
Write-Host "To test the workflow, upload a CSV file to the upload bucket:" -ForegroundColor Gray
Write-Host "  aws s3 cp test-data.csv s3://investtax-upload-$Stage/test@example.com/test-data.csv" -ForegroundColor Gray
Write-Host ""

# Return to original directory
Set-Location $RootDir
