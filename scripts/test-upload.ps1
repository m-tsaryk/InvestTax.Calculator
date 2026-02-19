# Test script to upload CSV file and trigger the InvestTax workflow
# This script uploads a CSV file to the S3 upload bucket, which triggers the Starter Lambda
# and initiates the Step Functions workflow

param(
    [Parameter(Mandatory=$false)]
    [string]$BucketName = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Email = "test@example.com",
    
    [Parameter(Mandatory=$false)]
    [string]$CsvFile = "test-data\sample.csv",
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "eu-central-1",
    
    [Parameter(Mandatory=$false)]
    [string]$Profile = $null,
    
    [Parameter(Mandatory=$false)]
    [switch]$Local = $true
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "InvestTax Calculator - Upload Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configure for Local or AWS
if ($Local) {
    # LocalStack configuration
    $endpoint = "http://localhost:4566"
    if ([string]::IsNullOrEmpty($BucketName)) {
        $BucketName = "investtax-upload-local"
    }
    
    # Set dummy credentials for LocalStack
    $env:AWS_ACCESS_KEY_ID = "test"
    $env:AWS_SECRET_ACCESS_KEY = "test"
    
    # Disable AWS CLI v2 advanced features not supported by LocalStack
    $env:AWS_S3_USE_SIGV4 = "true"
    $env:AWS_CLI_FILE_ENCODING = "utf-8"
    
    Write-Host "Mode: LOCAL (LocalStack)" -ForegroundColor Yellow
    Write-Host "Endpoint: $endpoint" -ForegroundColor Gray
}
else {
    # Real AWS configuration
    $endpoint = $null
    if ([string]::IsNullOrEmpty($BucketName)) {
        $BucketName = "investtax-upload-dev"
    }
    
    Write-Host "Mode: AWS (Real)" -ForegroundColor Yellow
}
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

Write-Host "Uploading file to S3..." -ForegroundColor Yellow

try {
    if ($Local) {
        # For LocalStack, use direct HTTP PUT to avoid AWS CLI v2 checksum issues
        $fileContent = [System.IO.File]::ReadAllBytes((Resolve-Path $CsvFile).Path)
        $uri = "$endpoint/$BucketName/$s3Key"
        
        $headers = @{
            "Content-Type" = "text/csv"
        }
        
        Invoke-RestMethod -Uri $uri -Method Put -Body $fileContent -Headers $headers -ErrorAction Stop | Out-Null
        $uploadSuccess = $true
    }
    else {
        # For real AWS, use standard AWS CLI
        $awsCommand = "aws s3 cp `"$CsvFile`" `"s3://$BucketName/$s3Key`" --region $Region"
        if ($Profile) {
            $awsCommand += " --profile $Profile"
        }
        Invoke-Expression $awsCommand
        $uploadSuccess = ($LASTEXITCODE -eq 0)
    }
    
    if ($uploadSuccess) {
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
