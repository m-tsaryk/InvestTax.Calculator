# Deploy Step Functions State Machine to LocalStack
# This script creates the Step Functions workflow in LocalStack

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deploying Step Functions" -ForegroundColor Cyan
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

$rootDir = Split-Path -Parent $PSScriptRoot
$stateMachineFile = Join-Path $rootDir "infrastructure\cdk\lib\state-machine-definition.json"

if (-not (Test-Path $stateMachineFile)) {
    Write-Host "[FAIL] State machine definition not found: $stateMachineFile" -ForegroundColor Red
    exit 1
}

Write-Host "Reading state machine definition..." -ForegroundColor Yellow
$stateMachineJson = Get-Content $stateMachineFile -Raw

# Replace CDK tokens with LocalStack values
$stateMachineJson = $stateMachineJson `
    -replace '\$\{JOBS_TABLE\}', 'InvestTax-Jobs-Local' `
    -replace '\$\{VALIDATOR_FUNCTION\}', 'InvestTax-Validator-Local' `
    -replace '\$\{NORMALIZER_FUNCTION\}', 'InvestTax-Normalizer-Local' `
    -replace '\$\{NBP_CLIENT_FUNCTION\}', 'InvestTax-NBPClient-Local' `
    -replace '\$\{CALCULATOR_FUNCTION\}', 'InvestTax-Calculator-Local' `
    -replace '\$\{REPORT_GENERATOR_FUNCTION\}', 'InvestTax-ReportGenerator-Local' `
    -replace '\$\{EMAIL_SENDER_FUNCTION\}', 'InvestTax-EmailSender-Local' `
    -replace '\$\{PROCESSING_BUCKET\}', 'investtax-processing-local'

# Save modified definition
$tempFile = "$env:TEMP\state-machine-local.json"
[System.IO.File]::WriteAllText($tempFile, $stateMachineJson)

Write-Host "[OK] State machine definition prepared" -ForegroundColor Green
Write-Host ""

# Create IAM role for Step Functions
Write-Host "Creating IAM role for Step Functions..." -ForegroundColor Yellow
try {
    $trustPolicy = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "states.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
"@
    
    aws --endpoint-url=$endpoint iam create-role `
        --role-name stepfunctions-role `
        --assume-role-policy-document $trustPolicy `
        --region $region 2>&1 | Out-Null
    
    Write-Host "  [OK] IAM role created" -ForegroundColor Green
}
catch {
    Write-Host "  [INFO] Role might already exist" -ForegroundColor Gray
}

Write-Host ""

# Check if state machine already exists
Write-Host "Checking for existing state machine..." -ForegroundColor Yellow
$stateMachineName = "InvestTax-Workflow-Local"
$stateMachineArn = "arn:aws:states:$region:000000000000:stateMachine:$stateMachineName"

$existing = aws --endpoint-url=$endpoint stepfunctions describe-state-machine `
    --state-machine-arn $stateMachineArn `
    --region $region 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "  State machine exists, deleting..." -ForegroundColor Gray
    aws --endpoint-url=$endpoint stepfunctions delete-state-machine `
        --state-machine-arn $stateMachineArn `
        --region $region 2>&1 | Out-Null
    Start-Sleep -Seconds 2
}

# Create state machine
Write-Host "Creating state machine..." -ForegroundColor Yellow
try {
    $result = aws --endpoint-url=$endpoint stepfunctions create-state-machine `
        --name $stateMachineName `
        --definition "file://$tempFile" `
        --role-arn "arn:aws:iam::000000000000:role/stepfunctions-role" `
        --region $region 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] State machine created successfully" -ForegroundColor Green
        Write-Host "  ARN: $stateMachineArn" -ForegroundColor Cyan
    } else {
        Write-Host "[FAIL] Failed to create state machine" -ForegroundColor Red
        Write-Host "Error: $result" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "[FAIL] Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}

Write-Host ""

# Configure S3 event notification to trigger Starter Lambda
Write-Host "Configuring S3 event notification..." -ForegroundColor Yellow
try {
    $notificationConfig = @"
{
  "LambdaFunctionConfigurations": [
    {
      "LambdaFunctionArn": "arn:aws:lambda:$region:000000000000:function:InvestTax-Starter-Local",
      "Events": ["s3:ObjectCreated:*"],
      "Filter": {
        "Key": {
          "FilterRules": [
            {
              "Name": "suffix",
              "Value": ".csv"
            }
          ]
        }
      }
    }
  ]
}
"@
    
    $notificationFile = "$env:TEMP\s3-notification.json"
    [System.IO.File]::WriteAllText($notificationFile, $notificationConfig)
    
    aws --endpoint-url=$endpoint s3api put-bucket-notification-configuration `
        --bucket investtax-upload-local `
        --notification-configuration "file://$notificationFile" `
        --region $region 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] S3 event notification configured" -ForegroundColor Green
    } else {
        Write-Host "[WARN] Failed to configure S3 notification (might not be critical)" -ForegroundColor Yellow
    }
    
    Remove-Item $notificationFile -ErrorAction SilentlyContinue
}
catch {
    Write-Host "[WARN] S3 notification configuration failed: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "       You may need to invoke the Starter Lambda manually" -ForegroundColor Gray
}

Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "State Machine ARN:" -ForegroundColor Yellow
Write-Host "  $stateMachineArn" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resources deployed:" -ForegroundColor Yellow
Write-Host "  - Lambda Functions: 7" -ForegroundColor White
Write-Host "  - Step Functions: InvestTax-Workflow-Local" -ForegroundColor White
Write-Host "  - S3 Event Notification: investtax-upload-local -> Starter Lambda" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Upload test file:  .\scripts\test-upload.ps1" -ForegroundColor White
Write-Host "  2. Verify workflow:   .\scripts\verify-workflow.ps1" -ForegroundColor White
Write-Host "  3. View DynamoDB:     http://localhost:8001" -ForegroundColor Cyan
Write-Host ""
