# Test Step Functions State Machine
# This script helps test the InvestTax workflow by uploading a test file or invoking the state machine directly

param(
    [string]$Stage = "dev",
    [string]$TestFile,
    [string]$Email = "test@example.com",
    [switch]$DirectInvoke,
    [string]$JobId
)

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "InvestTax Workflow Test" -ForegroundColor Cyan
Write-Host "Stage: $Stage" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

$uploadBucket = "investtax-upload-$Stage"
$processingBucket = "investtax-processing-$Stage"
$stateMachineArn = "arn:aws:states:eu-central-1:$(aws sts get-caller-identity --query Account --output text):stateMachine:InvestTax-Workflow-$Stage"

if ($DirectInvoke) {
    # Direct invocation of state machine (bypass S3 trigger)
    Write-Host "Direct State Machine Invocation" -ForegroundColor Yellow
    
    if (-not $JobId) {
        $JobId = [Guid]::NewGuid().ToString()
    }
    
    if (-not $TestFile) {
        Write-Host "Error: -TestFile parameter is required for direct invocation" -ForegroundColor Red
        exit 1
    }
    
    # Upload file to processing bucket directly
    $s3Key = "$Email/$([System.IO.Path]::GetFileName($TestFile))"
    Write-Host "  Uploading test file to s3://$uploadBucket/$s3Key..." -ForegroundColor Gray
    aws s3 cp $TestFile "s3://$uploadBucket/$s3Key"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Failed to upload test file" -ForegroundColor Red
        exit 1
    }
    
    # Create execution input
    $executionInput = @{
        JobId = $JobId
        Email = $Email
        S3Key = $s3Key
        UploadBucket = $uploadBucket
        ProcessingBucket = $processingBucket
    } | ConvertTo-Json -Compress
    
    Write-Host "  Starting execution with JobId: $JobId..." -ForegroundColor Gray
    
    $execution = aws stepfunctions start-execution `
        --state-machine-arn $stateMachineArn `
        --name $JobId `
        --input $executionInput `
        --output json | ConvertFrom-Json
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Failed to start execution" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "  ✓ Execution started successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "Execution ARN: $($execution.executionArn)" -ForegroundColor White
    Write-Host ""
    
    # Monitor execution
    Write-Host "Monitoring execution..." -ForegroundColor Yellow
    $maxWaitSeconds = 300
    $elapsedSeconds = 0
    
    while ($elapsedSeconds -lt $maxWaitSeconds) {
        Start-Sleep -Seconds 5
        $elapsedSeconds += 5
        
        $status = aws stepfunctions describe-execution `
            --execution-arn $execution.executionArn `
            --output json | ConvertFrom-Json
        
        $statusColor = switch ($status.status) {
            "RUNNING" { "Yellow" }
            "SUCCEEDED" { "Green" }
            "FAILED" { "Red" }
            "TIMED_OUT" { "Red" }
            "ABORTED" { "Red" }
            default { "Gray" }
        }
        
        Write-Host "  Status: $($status.status) (${elapsedSeconds}s elapsed)" -ForegroundColor $statusColor
        
        if ($status.status -ne "RUNNING") {
            break
        }
    }
    
    if ($status.status -eq "SUCCEEDED") {
        Write-Host ""
        Write-Host "✓ Execution completed successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Output:" -ForegroundColor White
        Write-Host $status.output -ForegroundColor Gray
    } elseif ($status.status -eq "FAILED") {
        Write-Host ""
        Write-Host "✗ Execution failed" -ForegroundColor Red
        Write-Host "Error: $($status.error)" -ForegroundColor Red
        Write-Host "Cause: $($status.cause)" -ForegroundColor Red
    } elseif ($status.status -eq "RUNNING") {
        Write-Host ""
        Write-Host "Execution still running after $maxWaitSeconds seconds" -ForegroundColor Yellow
        Write-Host "Check AWS Console for execution status: $($execution.executionArn)" -ForegroundColor Gray
    }
    
} else {
    # Test via S3 upload (triggers Lambda)
    Write-Host "S3 Upload Test (triggers workflow)" -ForegroundColor Yellow
    
    if (-not $TestFile) {
        Write-Host "Error: -TestFile parameter is required" -ForegroundColor Red
        Write-Host "Usage: .\test-workflow.ps1 -TestFile <path-to-csv> [-Email <email>] [-Stage <stage>]" -ForegroundColor Gray
        exit 1
    }
    
    if (-not (Test-Path $TestFile)) {
        Write-Host "Error: Test file not found: $TestFile" -ForegroundColor Red
        exit 1
    }
    
    $s3Key = "$Email/$([System.IO.Path]::GetFileName($TestFile))"
    
    Write-Host "  Uploading test file to s3://$uploadBucket/$s3Key..." -ForegroundColor Gray
    aws s3 cp $TestFile "s3://$uploadBucket/$s3Key"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Failed to upload test file" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "  ✓ File uploaded successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "Workflow has been triggered. Monitor execution in AWS Console:" -ForegroundColor White
    Write-Host "  https://console.aws.amazon.com/states/home?region=eu-central-1#/statemachines/view/$stateMachineArn" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Or check execution logs:" -ForegroundColor White
    Write-Host "  aws logs tail /aws/stepfunctions/InvestTax-Workflow-$Stage --follow" -ForegroundColor Gray
}

Write-Host ""
