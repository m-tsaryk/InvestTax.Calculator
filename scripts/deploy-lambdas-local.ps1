# Deploy Lambda functions to LocalStack
# This script deploys containerized Lambda functions to LocalStack

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deploying Lambdas to LocalStack" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set AWS credentials and endpoint
$env:AWS_ACCESS_KEY_ID = "test"
$env:AWS_SECRET_ACCESS_KEY = "test"
$endpoint = "http://localhost:4566"
$region = "eu-central-1"

# Check if LocalStack is running
try {
    $health = Invoke-RestMethod -Uri "$endpoint/_localstack/health" -TimeoutSec 5 -ErrorAction Stop
    Write-Host "[OK] LocalStack is running" -ForegroundColor Green
}
catch {
    Write-Host "[FAIL] LocalStack is not running!" -ForegroundColor Red
    Write-Host "Please start LocalStack first: .\scripts\start-local.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Create IAM role for Lambda first (LocalStack doesn't enforce permissions)
Write-Host "Creating IAM role for Lambda..." -ForegroundColor Yellow

$trustPolicy = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
"@

$ErrorActionPreference = "SilentlyContinue"
$null = aws --endpoint-url=$endpoint iam create-role `
    --role-name lambda-role `
    --assume-role-policy-document $trustPolicy `
    --region $region 2>&1
$roleCreateExitCode = $LASTEXITCODE
$ErrorActionPreference = "Stop"

if ($roleCreateExitCode -eq 0) {
    Write-Host "  [OK] IAM role created" -ForegroundColor Green
} else {
    Write-Host "  [INFO] Role might already exist" -ForegroundColor Gray
}

Write-Host ""

# Lambda functions configuration
$lambdaFunctions = @(
    @{ 
        Name = "InvestTax-Validator-Local"
        Image = "investtax-lambda-validator:latest"
        Handler = "not-used-with-image"
        Role = "arn:aws:iam::000000000000:role/lambda-role"
        Timeout = 60
        Memory = 512
        Environment = @{
            "AWS_ENDPOINT_URL" = $endpoint
            "S3_ENDPOINT" = $endpoint
            "DYNAMODB_ENDPOINT" = $endpoint
        }
    },
    @{ 
        Name = "InvestTax-Normalizer-Local"
        Image = "investtax-lambda-normalizer:latest"
        Handler = "not-used-with-image"
        Role = "arn:aws:iam::000000000000:role/lambda-role"
        Timeout = 120
        Memory = 512
        Environment = @{
            "AWS_ENDPOINT_URL" = $endpoint
            "S3_ENDPOINT" = $endpoint
        }
    },
    @{ 
        Name = "InvestTax-NBPClient-Local"
        Image = "investtax-lambda-nbpclient:latest"
        Handler = "not-used-with-image"
        Role = "arn:aws:iam::000000000000:role/lambda-role"
        Timeout = 60
        Memory = 256
        Environment = @{
            "NBP_API_URL" = "https://api.nbp.pl/api"
        }
    },
    @{ 
        Name = "InvestTax-Calculator-Local"
        Image = "investtax-lambda-calculator:latest"
        Handler = "not-used-with-image"
        Role = "arn:aws:iam::000000000000:role/lambda-role"
        Timeout = 120
        Memory = 512
        Environment = @{
            "AWS_ENDPOINT_URL" = $endpoint
            "S3_ENDPOINT" = $endpoint
        }
    },
    @{ 
        Name = "InvestTax-ReportGenerator-Local"
        Image = "investtax-lambda-reportgenerator:latest"
        Handler = "not-used-with-image"
        Role = "arn:aws:iam::000000000000:role/lambda-role"
        Timeout = 180
        Memory = 1024
        Environment = @{
            "AWS_ENDPOINT_URL" = $endpoint
            "S3_ENDPOINT" = $endpoint
        }
    },
    @{ 
        Name = "InvestTax-EmailSender-Local"
        Image = "investtax-lambda-emailsender:latest"
        Handler = "not-used-with-image"
        Role = "arn:aws:iam::000000000000:role/lambda-role"
        Timeout = 60
        Memory = 256
        Environment = @{
            "AWS_ENDPOINT_URL" = $endpoint
            "SES_ENDPOINT" = $endpoint
        }
    },
    @{ 
        Name = "InvestTax-Starter-Local"
        Image = "investtax-lambda-starter:latest"
        Handler = "not-used-with-image"
        Role = "arn:aws:iam::000000000000:role/lambda-role"
        Timeout = 60
        Memory = 256
        Environment = @{
            "AWS_ENDPOINT_URL" = $endpoint
            "DYNAMODB_ENDPOINT" = $endpoint
            "STATE_MACHINE_ARN" = "arn:aws:states:eu-central-1:000000000000:stateMachine:InvestTax-Workflow-Local"
        }
    }
)

$successCount = 0
$failCount = 0

foreach ($lambda in $lambdaFunctions) {
    $functionName = $lambda.Name
    Write-Host "Deploying $functionName..." -ForegroundColor Yellow
    
    # Check if function already exists and delete it
    $ErrorActionPreference = "SilentlyContinue"
    $null = aws --endpoint-url=$endpoint lambda get-function `
        --function-name $functionName `
        --region $region 2>&1
    $functionExists = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = "Stop"
    
    if ($functionExists) {
        Write-Host "  Function exists, deleting..." -ForegroundColor Gray
        $null = aws --endpoint-url=$endpoint lambda delete-function `
            --function-name $functionName `
            --region $region 2>&1
        Start-Sleep -Seconds 2
    }
    
    # Convert environment variables to JSON
    $envVars = @{ Variables = $lambda.Environment } | ConvertTo-Json -Compress -Depth 10
    
    # Create Lambda function with container image
    Write-Host "  Creating function with image $($lambda.Image)..." -ForegroundColor Gray
    
    $ErrorActionPreference = "SilentlyContinue"
    $createResult = aws --endpoint-url=$endpoint lambda create-function `
        --function-name $functionName `
        --package-type Image `
        --code "ImageUri=$($lambda.Image)" `
        --role $lambda.Role `
        --timeout $lambda.Timeout `
        --memory-size $lambda.Memory `
        --environment $envVars `
        --region $region 2>&1
    $createExitCode = $LASTEXITCODE
    $ErrorActionPreference = "Stop"
    
    if ($createExitCode -eq 0) {
        Write-Host "  [OK] Deployed successfully" -ForegroundColor Green
        $successCount++
    } else {
        Write-Host "  [FAIL] Deployment failed (Exit code: $createExitCode)" -ForegroundColor Red
        if ($createResult) {
            $createResult | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
        }
        $failCount++
    }
    
    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Successful: $successCount" -ForegroundColor Green
Write-Host "  Failed:     $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Gray" })
Write-Host ""

if ($successCount -eq $lambdaFunctions.Count) {
    Write-Host "All Lambda functions deployed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Deployed functions:" -ForegroundColor Yellow
    aws --endpoint-url=$endpoint lambda list-functions --region $region --query 'Functions[?contains(FunctionName, `InvestTax`)].FunctionName' --output table
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Deploy Step Functions: .\scripts\deploy-stepfunctions-local.ps1" -ForegroundColor White
    Write-Host "  2. Test workflow:         .\scripts\test-upload.ps1" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "Some deployments failed. Please check the errors above." -ForegroundColor Red
    exit 1
}
