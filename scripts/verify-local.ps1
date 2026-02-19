# Verify Local Development Environment
# Quick verification script for LocalStack setup

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Local Environment Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# 1. Check Prerequisites
Write-Host "1. Prerequisites:" -ForegroundColor Yellow
$dockerVer = docker --version 2>&1
$awsVer = aws --version 2>&1
$nodeVer = node --version 2>&1
$dotnetVer = dotnet --version 2>&1

if ($dockerVer -match "Docker") {
    Write-Host "   Success: Docker: $dockerVer" -ForegroundColor Green
} else {
    Write-Host "   Error: Docker not found" -ForegroundColor Red
    $allGood = $false
}

if ($awsVer -match "aws-cli") {
    $awsLine = $awsVer.Split([Environment]::NewLine)[0]
    Write-Host "   Success: AWS CLI: $awsLine" -ForegroundColor Green
} else {
    Write-Host "   Error: AWS CLI not found" -ForegroundColor Red
    $allGood = $false
}

if ($nodeVer -match "v") {
    Write-Host "   Success: Node.js: $nodeVer" -ForegroundColor Green
} else {
    Write-Host "   Error: Node.js not found" -ForegroundColor Red
    $allGood = $false
}

if ($dotnetVer) {
    Write-Host "   Success: .NET SDK: $dotnetVer" -ForegroundColor Green
} else {
    Write-Host "   Error: .NET SDK not found" -ForegroundColor Red
    $allGood = $false
}

# 2. Check Docker Containers
Write-Host ""
Write-Host "2. Docker Containers:" -ForegroundColor Yellow
$containers = docker ps --format "{{.Names}}" 2>&1

if ($containers -match "localstack") {
    Write-Host "   Success: LocalStack running" -ForegroundColor Green
} else {
    Write-Host "   Error: LocalStack not running" -ForegroundColor Red
    Write-Host "     Hint: Run .\scripts\start-local.ps1" -ForegroundColor Gray
    $allGood = $false
}

if ($containers -match "dynamodb-admin") {
    Write-Host "   Success: DynamoDB Admin running" -ForegroundColor Green
} else {
    Write-Host "   Warning: DynamoDB Admin not running" -ForegroundColor Yellow
}

# 3. Check LocalStack Health
Write-Host ""
Write-Host "3. LocalStack Services:" -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:4566/_localstack/health" -TimeoutSec 5 -ErrorAction Stop
    
    $services = @("s3", "dynamodb", "ses", "lambda", "stepfunctions")
    foreach ($svc in $services) {
        $status = $health.services.$svc
        if ($status -eq "available" -or $status -eq "running") {
            Write-Host "   Success: $svc : $status" -ForegroundColor Green
        } else {
            Write-Host "   Error: $svc : $status" -ForegroundColor Red
            $allGood = $false
        }
    }
}
catch {
    Write-Host "   Error: Cannot connect to LocalStack" -ForegroundColor Red
    $allGood = $false
}

# 4. Check AWS Resources  
Write-Host ""
Write-Host "4. AWS Resources:" -ForegroundColor Yellow

$buckets = aws --endpoint-url=http://localhost:4566 s3 ls 2>&1
if ($buckets -match "investtax-upload-local") {
    Write-Host "   Success: S3: investtax-upload-local" -ForegroundColor Green
} else {
    Write-Host "   Error: S3: investtax-upload-local missing" -ForegroundColor Red
    $allGood = $false
}

if ($buckets -match "investtax-processing-local") {
    Write-Host "   Success: S3: investtax-processing-local" -ForegroundColor Green
} else {
    Write-Host "   Error: S3: investtax-processing-local missing" -ForegroundColor Red
    $allGood = $false
}

$tables = aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-central-1 --output json 2>&1 | ConvertFrom-Json
if ($tables.TableNames -contains "InvestTax-Jobs-Local") {
    Write-Host "   Success: DynamoDB: InvestTax-Jobs-Local" -ForegroundColor Green
} else {
    Write-Host "   Error: DynamoDB: InvestTax-Jobs-Local missing" -ForegroundColor Red
    $allGood = $false
}

$identities = aws --endpoint-url=http://localhost:4566 ses list-identities --region eu-central-1 --output json 2>&1 | ConvertFrom-Json
if ($identities.Identities -contains "test@example.com") {
    Write-Host "   Success: SES: test@example.com verified" -ForegroundColor Green
} else {
    Write-Host "   Error: SES: test@example.com not verified" -ForegroundColor Red
    $allGood = $false
}

# 5. Check Web Interfaces
Write-Host ""
Write-Host "5. Web Interfaces:" -ForegroundColor Yellow
try {
    $ls = Invoke-WebRequest -Uri "http://localhost:4566/_localstack/health" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
    Write-Host "   Success: LocalStack at http://localhost:4566" -ForegroundColor Green
}
catch {
    Write-Host "   Error: LocalStack not accessible" -ForegroundColor Red
    $allGood = $false
}

try {
    $db = Invoke-WebRequest -Uri "http://localhost:8001" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
    Write-Host "   Success: DynamoDB Admin at http://localhost:8001" -ForegroundColor Green
}
catch {
    Write-Host "   Warning: DynamoDB Admin not accessible" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "  All Essential Checks Passed!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Your local environment is ready!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "  Issues Found" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To fix:" -ForegroundColor Yellow
    Write-Host "  1. Start Docker Desktop" -ForegroundColor White
    Write-Host "  2. Run: .\scripts\start-local.ps1" -ForegroundColor White
    Write-Host "  3. Initialize: .\infrastructure\localstack-init.ps1" -ForegroundColor White
    Write-Host ""
}
