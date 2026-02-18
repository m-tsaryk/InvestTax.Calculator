# Test script to upload CSV file and trigger the InvestTax workflow
# This script uploads a CSV file to the S3 upload bucket, which triggers the Starter Lambda
# and initiates the Step Functions workflow

param(
    [Parameter(Mandatory=$false)]
    [string]$BucketName = "investtax-upload-dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Email = "test@example.com",
    
    [Parameter(Mandatory=$false)]
    [string]$CsvFile = "test-data\sample.csv",
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "eu-central-1",
    
    [Parameter(Mandatory=$false)]
    [string]$Profile = $null
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "InvestTax Calculator - Upload Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validate CSV file exists
if (-not (Test-Path $CsvFile)) {
    Write-Host "ERROR: CSV file not found: $CsvFile" -ForegroundColor Red
    Write-Host "Please create a test CSV file or specify a valid path." -ForegroundColor Red
    exit 1
}

# Generate unique filename with timestamp
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$originalFilename = Split-Path $CsvFile -Leaf
$filenameWithoutExt = [System.IO.Path]::GetFileNameWithoutExtension($originalFilename)
$extension = [System.IO.Path]::GetExtension($originalFilename)
$newFilename = "$filenameWithoutExt-$timestamp$extension"

# S3 key format: email@example.com/filename.csv
# This format is expected by the Starter Lambda function
$s3Key = "$Email/$newFilename"

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Bucket:       $BucketName"
Write-Host "  Email:        $Email"
Write-Host "  Source File:  $CsvFile"
Write-Host "  S3 Key:       $s3Key"
Write-Host "  Region:       $Region"
Write-Host ""

# Build AWS CLI command
$awsCommand = "aws s3 cp `"$CsvFile`" `"s3://$BucketName/$s3Key`" --region $Region"

if ($Profile) {
    $awsCommand += " --profile $Profile"
    Write-Host "  Profile:      $Profile"
}

Write-Host ""
Write-Host "Uploading file to S3..." -ForegroundColor Yellow

try {
    # Execute upload
    Invoke-Expression $awsCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Upload Successful!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "The file has been uploaded and should trigger the workflow." -ForegroundColor White
        Write-Host ""
        Write-Host "Monitoring:" -ForegroundColor Yellow
        Write-Host "  - Check Step Functions console for execution status"
        Write-Host "  - View CloudWatch Logs for Lambda function logs"
        Write-Host "  - Query DynamoDB Jobs table for job status"
        Write-Host ""
        Write-Host "Email:        $Email" -ForegroundColor Cyan
        Write-Host "S3 Location:  s3://$BucketName/$s3Key" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "To check Step Functions executions:" -ForegroundColor Gray
        Write-Host "  aws stepfunctions list-executions --state-machine-arn <state-machine-arn> --region $Region" -ForegroundColor Gray
        Write-Host ""
    }
    else {
        Write-Host ""
        Write-Host "Upload failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        Write-Host "Please check your AWS credentials and bucket permissions." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
