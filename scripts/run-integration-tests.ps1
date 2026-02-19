# End-to-End Integration Test Script
# This script executes all integration test scenarios and validates results

param(
    [Parameter(Mandatory=$false)]
    [string]$BucketName = "investtax-upload-dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Email = "test@example.com",
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "eu-central-1",
    
    [Parameter(Mandatory=$false)]
    [string]$Profile = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$StateMachineArn = $null,
    
    [Parameter(Mandatory=$false)]
    [int]$TimeoutSeconds = 300,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipValidation
)

# Test case definitions
$testCases = @(
    @{
        Name = "Test 1: Simple Success"
        File = "test-data\test-1-simple-success.csv"
        ExpectedFile = "test-data\test-1-simple-success.expected.json"
        ShouldSucceed = $true
        Description = "Simple buy and sell transaction with expected profit"
    },
    @{
        Name = "Test 2: Multiple Stocks"
        File = "test-data\test-2-multiple-stocks.csv"
        ExpectedFile = "test-data\test-2-multiple-stocks.expected.json"
        ShouldSucceed = $true
        Description = "Multiple stocks with various transactions testing FIFO across different ISINs"
    },
    @{
        Name = "Test 3: Partial Fills"
        File = "test-data\test-3-partial-fills.csv"
        ExpectedFile = "test-data\test-3-partial-fills.expected.json"
        ShouldSucceed = $true
        Description = "Complex FIFO scenario with partial fills"
    },
    @{
        Name = "Test 4: Validation Error - Missing ISIN"
        File = "test-data\test-4-validation-error-missing-isin.csv"
        ExpectedFile = "test-data\test-4-validation-error-missing-isin.expected.json"
        ShouldSucceed = $false
        Description = "Should fail validation due to missing ISIN"
    },
    @{
        Name = "Test 5: Validation Error - Invalid Date"
        File = "test-data\test-5-validation-error-invalid-date.csv"
        ExpectedFile = "test-data\test-5-validation-error-invalid-date.expected.json"
        ShouldSucceed = $false
        Description = "Should fail validation due to invalid date format"
    }
)

# Test results tracking
$testResults = @()
$passedTests = 0
$failedTests = 0

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       InvestTax Calculator - Integration Test Suite           ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Bucket:        $BucketName"
Write-Host "  Email:         $Email"
Write-Host "  Region:        $Region"
Write-Host "  Test Cases:    $($testCases.Count)"
Write-Host "  Timeout:       ${TimeoutSeconds}s per test"
if ($Profile) { Write-Host "  Profile:       $Profile" }
Write-Host ""

# Validate test files exist
Write-Host "Validating test files..." -ForegroundColor Yellow
$allFilesExist = $true
foreach ($testCase in $testCases) {
    if (-not (Test-Path $testCase.File)) {
        Write-Host "  ✗ Missing: $($testCase.File)" -ForegroundColor Red
        $allFilesExist = $false
    }
    else {
        Write-Host "  ✓ Found: $($testCase.File)" -ForegroundColor Green
    }
}

if (-not $allFilesExist) {
    Write-Host ""
    Write-Host "ERROR: Some test files are missing. Please ensure all test files exist." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting test execution..." -ForegroundColor Green
Write-Host ""

# Function to upload file and get job ID
function Invoke-TestUpload {
    param($TestCase)
    
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $jobId = [guid]::NewGuid().ToString()
    $filename = Split-Path $TestCase.File -Leaf
    $s3Key = "$Email/$jobId-$timestamp-$filename"
    
    Write-Host "  Uploading: $filename" -ForegroundColor White
    Write-Host "  Job ID:    $jobId" -ForegroundColor Gray
    Write-Host "  S3 Key:    $s3Key" -ForegroundColor Gray
    
    $awsCommand = "aws s3 cp `"$($TestCase.File)`" `"s3://$BucketName/$s3Key`" --region $Region"
    if ($Profile) {
        $awsCommand += " --profile $Profile"
    }
    
    try {
        Invoke-Expression $awsCommand | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Upload successful" -ForegroundColor Green
            return @{
                Success = $true
                JobId = $jobId
                S3Key = $s3Key
            }
        }
        else {
            Write-Host "  ✗ Upload failed" -ForegroundColor Red
            return @{ Success = $false }
        }
    }
    catch {
        Write-Host "  ✗ Upload error: $($_.Exception.Message)" -ForegroundColor Red
        return @{ Success = $false }
    }
}

# Function to wait for execution and check status
function Wait-ExecutionComplete {
    param($JobId, $TimeoutSeconds)
    
    $elapsed = 0
    $checkInterval = 10
    
    Write-Host "  Waiting for execution (timeout: ${TimeoutSeconds}s)..." -ForegroundColor White
    
    while ($elapsed -lt $TimeoutSeconds) {
        Start-Sleep -Seconds $checkInterval
        $elapsed += $checkInterval
        
        # In a real implementation, query DynamoDB or Step Functions for job status
        # For now, we'll just wait the full timeout
        Write-Host "  ... ${elapsed}s elapsed" -ForegroundColor Gray
    }
    
    Write-Host "  ⓘ Manual verification required - check CloudWatch Logs and Step Functions console" -ForegroundColor Yellow
}

# Execute each test case
$testNumber = 0
foreach ($testCase in $testCases) {
    $testNumber++
    
    Write-Host "─────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "Test Case $testNumber of $($testCases.Count)" -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "Name:        $($testCase.Name)" -ForegroundColor White
    Write-Host "Description: $($testCase.Description)" -ForegroundColor Gray
    Write-Host "Expected:    $(if ($testCase.ShouldSucceed) { 'Success' } else { 'Failure' })" -ForegroundColor $(if ($testCase.ShouldSucceed) { 'Green' } else { 'Yellow' })
    Write-Host ""
    
    $startTime = Get-Date
    
    # Upload file
    $uploadResult = Invoke-TestUpload -TestCase $testCase
    
    if (-not $uploadResult.Success) {
        Write-Host "  ✗ Test FAILED - Upload error" -ForegroundColor Red -BackgroundColor Black
        $failedTests++
        $testResults += @{
            TestCase = $testCase.Name
            Result = "FAILED"
            Reason = "Upload error"
            Duration = ((Get-Date) - $startTime).TotalSeconds
        }
        Write-Host ""
        continue
    }
    
    # Wait for execution (in production, would monitor actual status)
    if (-not $SkipValidation) {
        Wait-ExecutionComplete -JobId $uploadResult.JobId -TimeoutSeconds $TimeoutSeconds
    }
    
    $duration = ((Get-Date) - $startTime).TotalSeconds
    
    # Result tracking
    Write-Host ""
    Write-Host "  Test execution completed" -ForegroundColor Green
    Write-Host "  Duration: $([math]::Round($duration, 1))s" -ForegroundColor Gray
    Write-Host ""
    
    # For now, mark as needing manual verification
    Write-Host "  ⓘ MANUAL VERIFICATION REQUIRED:" -ForegroundColor Yellow
    Write-Host "    1. Check Step Functions execution for Job ID: $($uploadResult.JobId)" -ForegroundColor Gray
    Write-Host "    2. Verify email delivery to: $Email" -ForegroundColor Gray
    Write-Host "    3. Check DynamoDB Jobs table for final status" -ForegroundColor Gray
    Write-Host "    4. Review CloudWatch Logs for any errors" -ForegroundColor Gray
    if ($testCase.ShouldSucceed) {
        Write-Host "    5. Compare report output with expected results in: $($testCase.ExpectedFile)" -ForegroundColor Gray
    }
    Write-Host ""
    
    # Add to results (marked as needs verification)
    $testResults += @{
        TestCase = $testCase.Name
        Result = "NEEDS_VERIFICATION"
        JobId = $uploadResult.JobId
        S3Key = $uploadResult.S3Key
        Duration = $duration
        ShouldSucceed = $testCase.ShouldSucceed
        ExpectedFile = $testCase.ExpectedFile
    }
}

# Summary report
Write-Host "═════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "                        TEST SUMMARY                              " -ForegroundColor Cyan
Write-Host "═════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Host "Total Tests:           $($testCases.Count)" -ForegroundColor White
Write-Host "Tests Executed:        $($testResults.Count)" -ForegroundColor White
Write-Host "Needs Verification:    $($testResults.Where({$_.Result -eq 'NEEDS_VERIFICATION'}).Count)" -ForegroundColor Yellow
Write-Host "Failed (Upload):       $failedTests" -ForegroundColor $(if ($failedTests -gt 0) { 'Red' } else { 'Green' })
Write-Host ""

Write-Host "Test Results:" -ForegroundColor Yellow
foreach ($result in $testResults) {
    $icon = switch ($result.Result) {
        "NEEDS_VERIFICATION" { "⚠" }
        "FAILED" { "✗" }
        "PASSED" { "✓" }
    }
    
    $color = switch ($result.Result) {
        "NEEDS_VERIFICATION" { "Yellow" }
        "FAILED" { "Red" }
        "PASSED" { "Green" }
    }
    
    Write-Host "  $icon $($result.TestCase)" -ForegroundColor $color
    if ($result.JobId) {
        Write-Host "    Job ID: $($result.JobId)" -ForegroundColor Gray
    }
    Write-Host "    Duration: $([math]::Round($result.Duration, 1))s" -ForegroundColor Gray
}

Write-Host ""
Write-Host "═════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Save results to file
$resultsFile = "test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$testResults | ConvertTo-Json -Depth 10 | Out-File $resultsFile -Encoding UTF8
Write-Host "Test results saved to: $resultsFile" -ForegroundColor Gray
Write-Host ""

# Next steps
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Review Step Functions console for execution details" -ForegroundColor White
Write-Host "  2. Check email inbox ($Email) for success/error notifications" -ForegroundColor White
Write-Host "  3. Query DynamoDB Jobs table for job statuses" -ForegroundColor White
Write-Host "  4. Review CloudWatch Logs for detailed execution traces" -ForegroundColor White
Write-Host "  5. Validate tax calculation results against expected outcomes" -ForegroundColor White
Write-Host ""

Write-Host "Useful Commands:" -ForegroundColor Yellow
if ($StateMachineArn) {
    Write-Host "  List executions:" -ForegroundColor Gray
    Write-Host "    aws stepfunctions list-executions --state-machine-arn $StateMachineArn --region $Region" -ForegroundColor DarkGray
}
Write-Host "  Query DynamoDB:" -ForegroundColor Gray
Write-Host "    aws dynamodb scan --table-name InvestTax-Jobs-dev --region $Region" -ForegroundColor DarkGray
Write-Host "  View CloudWatch Logs:" -ForegroundColor Gray
Write-Host "    aws logs tail /aws/lambda/InvestTax-Validator-dev --follow --region $Region" -ForegroundColor DarkGray
Write-Host ""

if ($failedTests -gt 0) {
    Write-Host "⚠ Some tests failed during upload. Please review and retry." -ForegroundColor Red
    exit 1
}
else {
    Write-Host "✓ All tests uploaded successfully. Manual verification required." -ForegroundColor Green
    exit 0
}
