# Verify Workflow Execution
# This script checks the status of uploaded files and workflow results

param(
    [Parameter(Mandatory=$false)]
    [string]$Email = "test@example.com",
    
    [Parameter(Mandatory=$false)]
    [switch]$Local = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$JobId = $null
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Workflow Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configure endpoint
if ($Local) {
    $endpoint = "--endpoint-url=http://localhost:4566"
    $region = "eu-central-1"
    $tableName = "InvestTax-Jobs-Local"
    $uploadBucket = "investtax-upload-local"
    $processingBucket = "investtax-processing-local"
    
    # Set dummy credentials for LocalStack
    $env:AWS_ACCESS_KEY_ID = "test"
    $env:AWS_SECRET_ACCESS_KEY = "test"
    
    Write-Host "Mode: LOCAL (LocalStack)" -ForegroundColor Yellow
    Write-Host "Endpoint: http://localhost:4566" -ForegroundColor Gray
}
else {
    $endpoint = ""
    $region = "eu-central-1"
    $tableName = "InvestTax-Jobs-dev"
    $uploadBucket = "investtax-upload-dev"
    $processingBucket = "investtax-processing-dev"
    
    Write-Host "Mode: AWS (Real)" -ForegroundColor Yellow
}
Write-Host ""

# 1. Check DynamoDB for recent jobs
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Checking DynamoDB Jobs Table" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    if ($JobId) {
        # Get specific job
        Write-Host "Fetching job: $JobId" -ForegroundColor Gray
        $jobJson = aws $endpoint dynamodb get-item `
            --table-name $tableName `
            --key "{`"JobId`":{`"S`":`"$JobId`"}}" `
            --region $region 2>&1
    }
    else {
        # Scan for recent jobs (last 10)
        Write-Host "Scanning for recent jobs..." -ForegroundColor Gray
        $jobJson = aws $endpoint dynamodb scan `
            --table-name $tableName `
            --limit 10 `
            --region $region 2>&1
    }
    
    if ($LASTEXITCODE -eq 0) {
        $result = $jobJson | ConvertFrom-Json
        
        if ($JobId -and $result.Item) {
            # Single job result
            $job = $result.Item
            Write-Host "Job Found!" -ForegroundColor Green
            Write-Host "  Job ID:       $($job.JobId.S)" -ForegroundColor White
            Write-Host "  Email:        $($job.Email.S)" -ForegroundColor White
            Write-Host "  Status:       $($job.Status.S)" -ForegroundColor $(if ($job.Status.S -eq "COMPLETED") { "Green" } elseif ($job.Status.S -eq "FAILED") { "Red" } else { "Yellow" })
            Write-Host "  Created:      $($job.CreatedAt.S)" -ForegroundColor White
            if ($job.UpdatedAt) {
                Write-Host "  Updated:      $($job.UpdatedAt.S)" -ForegroundColor White
            }
            if ($job.ErrorMessage) {
                Write-Host "  Error:        $($job.ErrorMessage.S)" -ForegroundColor Red
            }
            if ($job.ReportS3Key) {
                Write-Host "  Report:       s3://$processingBucket/$($job.ReportS3Key.S)" -ForegroundColor Cyan
            }
        }
        elseif ($result.Items) {
            # Multiple jobs
            Write-Host "Found $($result.Items.Count) recent jobs:" -ForegroundColor Green
            Write-Host ""
            
            $result.Items | Sort-Object { $_.CreatedAt.S } -Descending | ForEach-Object {
                $statusColor = switch ($_.Status.S) {
                    "COMPLETED" { "Green" }
                    "FAILED" { "Red" }
                    "PROCESSING" { "Yellow" }
                    default { "White" }
                }
                
                Write-Host "  Job: $($_.JobId.S)" -ForegroundColor Cyan
                Write-Host "    Email:  $($_.Email.S)"
                Write-Host "    Status: $($_.Status.S)" -ForegroundColor $statusColor
                Write-Host "    Date:   $($_.CreatedAt.S)"
                if ($_.ErrorMessage) {
                    Write-Host "    Error:  $($_.ErrorMessage.S)" -ForegroundColor Red
                }
                Write-Host ""
            }
        }
        else {
            Write-Host "No jobs found" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Error querying DynamoDB:" -ForegroundColor Red
        Write-Host $jobJson -ForegroundColor Red
    }
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# 2. Check S3 Upload Bucket
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "2. Checking S3 Upload Bucket" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    $uploadFiles = aws $endpoint s3 ls "s3://$uploadBucket/$Email/" --recursive --region $region 2>&1
    
    if ($LASTEXITCODE -eq 0 -and $uploadFiles) {
        Write-Host "Uploaded files for $Email" -ForegroundColor Green
        $uploadFiles -split "`n" | Where-Object { $_ } | ForEach-Object {
            Write-Host "  $_" -ForegroundColor White
        }
    }
    else {
        Write-Host "No files found in upload bucket for $Email" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Error checking upload bucket: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Check S3 Processing Bucket for Reports
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "3. Checking Generated Reports" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    $reportFiles = aws $endpoint s3 ls "s3://$processingBucket/reports/$Email/" --recursive --region $region 2>&1
    
    if ($LASTEXITCODE -eq 0 -and $reportFiles) {
        Write-Host "Generated reports for $Email" -ForegroundColor Green
        $reportFiles -split "`n" | Where-Object { $_ } | ForEach-Object {
            Write-Host "  $_" -ForegroundColor White
        }
        
        # Get the most recent report
        $latestReport = ($reportFiles -split "`n" | Where-Object { $_ } | Select-Object -Last 1)
        if ($latestReport -match "s3://.*/(reports/.*)") {
            $reportKey = $Matches[1]
        }
        elseif ($latestReport -match "\s+(reports/\S+)") {
            $reportKey = $Matches[1]
        }
        
        if ($reportKey) {
            Write-Host ""
            Write-Host "Download latest report:" -ForegroundColor Yellow
            Write-Host "  aws $endpoint s3 cp s3://$processingBucket/$reportKey ./report.pdf --region $region" -ForegroundColor Gray
        }
    }
    else {
        Write-Host "No reports found for $Email" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Error checking reports: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Check LocalStack SES Email Logs (Local only)
if ($Local) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "4. Checking Email Logs (LocalStack)" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "Note: LocalStack doesn't actually send emails." -ForegroundColor Yellow
    Write-Host "Check Docker logs for SES email activity:" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  docker logs investtax-calculator-localstack-1 2>&1 | Select-String -Pattern 'SES|email'" -ForegroundColor White
    Write-Host ""
}

# 5. Quick access to DynamoDB Admin UI
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Access" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "DynamoDB Admin UI (view/edit data visually):" -ForegroundColor Yellow
Write-Host "  http://localhost:8001" -ForegroundColor Cyan
Write-Host ""
Write-Host "Query specific job:" -ForegroundColor Yellow
Write-Host "  .\scripts\verify-workflow.ps1 -JobId <job-id>" -ForegroundColor White
Write-Host ""
Write-Host "Download a report:" -ForegroundColor Yellow
Write-Host "  aws $endpoint s3 cp s3://$processingBucket/reports/$Email/<filename> ./report.pdf --region $region" -ForegroundColor White
Write-Host ""

Write-Host "========================================`n" -ForegroundColor Cyan
