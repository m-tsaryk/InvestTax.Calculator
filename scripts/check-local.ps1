# Check Local Development Environment Status
# This script checks the status of LocalStack and AWS resources

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LocalStack Environment Status" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Check if containers are running
Write-Host "Docker Containers:" -ForegroundColor Yellow
$containers = docker ps --filter "name=investtax" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
if ($containers) {
    Write-Host $containers -ForegroundColor White
} else {
    Write-Host "  No containers running" -ForegroundColor Red
    Write-Host "`nTo start the environment, run:" -ForegroundColor Yellow
    Write-Host "  .\scripts\start-local.ps1`n" -ForegroundColor White
    exit 0
}

# Check LocalStack health
Write-Host "`nLocalStack Health:" -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:4566/_localstack/health" -TimeoutSec 5
    $health.services.PSObject.Properties | ForEach-Object {
        $service = $_.Name
        $status = $_.Value
        if ($status -eq "running" -or $status -eq "available") {
            Write-Host "  ✓ $service : $status" -ForegroundColor Green
        } else {
            Write-Host "  ✗ $service : $status" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "  ERROR: Cannot connect to LocalStack" -ForegroundColor Red
}

# Check AWS CLI
Write-Host "`nAWS CLI:" -ForegroundColor Yellow
$awsInstalled = Get-Command aws -ErrorAction SilentlyContinue
if ($null -eq $awsInstalled) {
    Write-Host "  ✗ AWS CLI not installed" -ForegroundColor Red
    Write-Host "  Install from: https://awscli.amazonaws.com/AWSCLIV2.msi" -ForegroundColor Gray
} else {
    $awsVersion = aws --version 2>&1
    Write-Host "  ✓ $awsVersion" -ForegroundColor Green
    
    # Check S3 buckets
    Write-Host "`nS3 Buckets:" -ForegroundColor Yellow
    try {
        $buckets = aws --endpoint-url=http://localhost:4566 s3 ls 2>&1
        if ($LASTEXITCODE -eq 0) {
            $buckets -split "`n" | Where-Object { $_ -match "investtax" } | ForEach-Object {
                Write-Host "  ✓ $_" -ForegroundColor Green
            }
        } else {
            Write-Host "  No buckets found or error accessing S3" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  Error checking S3 buckets" -ForegroundColor Red
    }
    
    # Check DynamoDB tables
    Write-Host "`nDynamoDB Tables:" -ForegroundColor Yellow
    try {
        $tables = aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-central-1 2>&1 | ConvertFrom-Json
        if ($tables.TableNames) {
            $tables.TableNames | Where-Object { $_ -match "InvestTax" } | ForEach-Object {
                Write-Host "  ✓ $_" -ForegroundColor Green
            }
        } else {
            Write-Host "  No tables found" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  Error checking DynamoDB tables" -ForegroundColor Red
    }
}

# Display URLs
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Available Services:" -ForegroundColor Yellow
Write-Host "  • LocalStack:      http://localhost:4566" -ForegroundColor White
Write-Host "  • DynamoDB Admin:  http://localhost:8001" -ForegroundColor White
Write-Host "  • Health Check:    http://localhost:4566/_localstack/health" -ForegroundColor White
Write-Host "`n========================================`n" -ForegroundColor Cyan
