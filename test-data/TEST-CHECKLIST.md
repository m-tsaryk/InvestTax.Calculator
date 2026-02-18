# InvestTax Calculator - End-to-End Integration Test Checklist

**Version**: 1.0  
**Last Updated**: February 19, 2026  
**Test Environment**: Development (dev)  

---

## Table of Contents

1. [Pre-Test Setup Verification](#pre-test-setup-verification)
2. [Test Environment Configuration](#test-environment-configuration)
3. [Test Case Execution](#test-case-execution)
4. [Results Verification](#results-verification)
5. [Performance Benchmarks](#performance-benchmarks)
6. [Post-Test Cleanup](#post-test-cleanup)

---

## Pre-Test Setup Verification

### Infrastructure Checks

Before running integration tests, verify the following infrastructure components are deployed and operational:

#### ✅ AWS Resources

- [ ] **S3 Buckets Created**
  - [ ] Upload bucket: `investtax-upload-dev` exists
  - [ ] Processing bucket: `investtax-processing-dev` exists
  - [ ] EventBridge notifications enabled on upload bucket
  
- [ ] **DynamoDB Table Created**
  - [ ] Table name: `InvestTax-Jobs-dev`
  - [ ] Primary key: `jobId` (String)
  - [ ] Billing mode: On-Demand
  - [ ] Table accessible via AWS CLI
  
- [ ] **Lambda Functions Deployed**
  - [ ] InvestTax-Starter-dev
  - [ ] InvestTax-Validator-dev
  - [ ] InvestTax-Normalizer-dev
  - [ ] InvestTax-NBPClient-dev
  - [ ] InvestTax-Calculator-dev
  - [ ] InvestTax-ReportGenerator-dev
  - [ ] InvestTax-EmailSender-dev
  
- [ ] **Step Functions State Machine**
  - [ ] State machine: `InvestTax-Workflow-dev` deployed
  - [ ] All Lambda integrations configured
  - [ ] Error handlers defined
  - [ ] CloudWatch Logs enabled
  - [ ] X-Ray tracing enabled
  
- [ ] **IAM Permissions**
  - [ ] Lambda execution roles have required permissions
  - [ ] Step Functions can invoke all Lambdas
  - [ ] SES send email permissions granted
  
- [ ] **Amazon SES Configuration**
  - [ ] Sender email verified: `noreply@investtax.example.com`
  - [ ] Test recipient email verified: (your test email)
  - [ ] SES moved out of sandbox OR test emails verified
  - [ ] SMTP settings configured (if applicable)

#### ✅ Local Environment

- [ ] **AWS CLI Configured**
  ```powershell
  aws --version  # Should be v2.15.x or later
  aws sts get-caller-identity  # Verify credentials
  ```
  
- [ ] **Test Data Files Present**
  - [ ] `test-data/test-1-simple-success.csv`
  - [ ] `test-data/test-2-multiple-stocks.csv`
  - [ ] `test-data/test-3-partial-fills.csv`
  - [ ] `test-data/test-4-validation-error-missing-isin.csv`
  - [ ] `test-data/test-5-validation-error-invalid-date.csv`
  
- [ ] **Expected Results Files Present**
  - [ ] `test-data/test-1-simple-success.expected.json`
  - [ ] `test-data/test-2-multiple-stocks.expected.json`
  - [ ] `test-data/test-3-partial-fills.expected.json`
  - [ ] `test-data/test-4-validation-error-missing-isin.expected.json`
  - [ ] `test-data/test-5-validation-error-invalid-date.expected.json`
  
- [ ] **Test Scripts Available**
  - [ ] `scripts/run-integration-tests.ps1`
  - [ ] `scripts/test-upload.ps1`

---

## Test Environment Configuration

### Environment Variables

Configure the following parameters before running tests:

```powershell
# Required parameters
$BucketName = "investtax-upload-dev"
$Email = "your-test-email@example.com"  # Must be verified in SES
$Region = "eu-central-1"

# Optional parameters
$Profile = "default"  # AWS CLI profile name
$StateMachineArn = "arn:aws:states:eu-central-1:ACCOUNT_ID:stateMachine:InvestTax-Workflow-dev"
```

### Verification Commands

Run these commands to verify environment setup:

```powershell
# Verify S3 buckets
aws s3 ls | Select-String "investtax"

# Verify DynamoDB table
aws dynamodb describe-table --table-name InvestTax-Jobs-dev --region eu-central-1

# Verify Lambda functions
aws lambda list-functions --region eu-central-1 | Select-String "InvestTax"

# Verify Step Functions state machine
aws stepfunctions list-state-machines --region eu-central-1 | Select-String "InvestTax"

# Verify SES sender email
aws ses list-verified-email-addresses --region eu-central-1
```

---

## Test Case Execution

### Running All Tests

Execute the complete test suite:

```powershell
cd d:\Projects\InvestTax.Calculator

.\scripts\run-integration-tests.ps1 `
    -BucketName "investtax-upload-dev" `
    -Email "your-test-email@example.com" `
    -Region "eu-central-1" `
    -TimeoutSeconds 300
```

### Running Individual Tests

Execute a single test case:

```powershell
.\scripts\test-upload.ps1 `
    -BucketName "investtax-upload-dev" `
    -Email "your-test-email@example.com" `
    -CsvFile "test-data\test-1-simple-success.csv" `
    -Region "eu-central-1"
```

### Test Case Details

#### Test 1: Simple Success ✅

**File**: `test-1-simple-success.csv`  
**Expected Outcome**: Success with tax calculation  
**Validation Points**:
- [ ] CSV file uploaded successfully
- [ ] Validation passed (no errors)
- [ ] Data normalized correctly
- [ ] NBP rates fetched for USD on transaction dates
- [ ] Tax calculated correctly (Gain: ~1455 PLN, Tax: ~276.45 PLN at 19%)
- [ ] Plain text report generated
- [ ] Success email sent with report attachment
- [ ] Job status in DynamoDB: `Completed`
- [ ] Total execution time < 5 minutes

**Manual Verification**:
```powershell
# Check DynamoDB for job status
aws dynamodb scan --table-name InvestTax-Jobs-dev --region eu-central-1 | Select-String "jobId"

# Download generated report
aws s3 ls s3://investtax-processing-dev/reports/ --region eu-central-1
aws s3 cp s3://investtax-processing-dev/reports/JOB_ID.txt ./test-results/ --region eu-central-1
```

---

#### Test 2: Multiple Stocks ✅

**File**: `test-2-multiple-stocks.csv`  
**Expected Outcome**: Success with multiple ISIN calculations  
**Validation Points**:
- [ ] All ISINs processed (AAPL, MSFT, GOOGL)
- [ ] FIFO applied separately per ISIN
- [ ] Partial AAPL sell matched correctly
- [ ] All NBP rates fetched
- [ ] Tax calculated for all stocks
- [ ] Report shows breakdown by ISIN
- [ ] Success email received
- [ ] Execution time < 5 minutes

---

#### Test 3: Partial Fills ✅

**File**: `test-3-partial-fills.csv`  
**Expected Outcome**: Success with complex FIFO matching  
**Validation Points**:
- [ ] First sell (15 shares) consumes entire first buy (20) partially
- [ ] Second sell (8 shares) finishes first buy (5) + starts second buy (10)
- [ ] Third sell (10 shares) finishes second buy + starts third buy
- [ ] Remaining positions tracked correctly
- [ ] Report shows matched transactions with partial indicators
- [ ] Success email received
- [ ] Execution time < 5 minutes

---

#### Test 4: Validation Error - Missing ISIN ❌

**File**: `test-4-validation-error-missing-isin.csv`  
**Expected Outcome**: Validation failure with error email  
**Validation Points**:
- [ ] CSV file uploaded successfully
- [ ] Validation detected missing ISIN on row 2
- [ ] Workflow stopped after validation stage
- [ ] Error email sent with validation details
- [ ] Job status in DynamoDB: `Failed`
- [ ] Error message includes: "ISIN is required"
- [ ] No subsequent Lambda invocations (NBP, Calculator, etc.)
- [ ] Execution time < 2 minutes

**Error Email Content Verification**:
- [ ] Subject: "InvestTax Calculator - Processing Failed"
- [ ] Body contains error details
- [ ] Actionable guidance provided
- [ ] Job ID referenced

---

#### Test 5: Validation Error - Invalid Date ❌

**File**: `test-5-validation-error-invalid-date.csv`  
**Expected Outcome**: Validation failure with error email  
**Validation Points**:
- [ ] CSV file uploaded successfully
- [ ] Validation detected invalid date format on row 2
- [ ] Workflow stopped after validation stage
- [ ] Error email sent with validation details
- [ ] Job status in DynamoDB: `Failed`
- [ ] Error message includes: "Invalid date format"
- [ ] No subsequent Lambda invocations
- [ ] Execution time < 2 minutes

---

## Results Verification

### CloudWatch Logs Verification

Check logs for each Lambda function to ensure proper execution:

```powershell
# View logs for Validator Lambda
aws logs tail /aws/lambda/InvestTax-Validator-dev --follow --region eu-central-1

# View logs for Calculator Lambda
aws logs tail /aws/lambda/InvestTax-Calculator-dev --follow --region eu-central-1

# View logs for EmailSender Lambda
aws logs tail /aws/lambda/InvestTax-EmailSender-dev --follow --region eu-central-1
```

**Log Verification Checklist**:
- [ ] No ERROR level log entries for successful tests
- [ ] Expected ERROR entries for validation failure tests
- [ ] All Lambda invocations logged
- [ ] Request IDs present for tracing
- [ ] Execution duration logged

---

### Step Functions Execution Verification

Monitor Step Functions console:

```powershell
# List recent executions
aws stepfunctions list-executions `
    --state-machine-arn arn:aws:states:eu-central-1:ACCOUNT_ID:stateMachine:InvestTax-Workflow-dev `
    --region eu-central-1 `
    --max-results 10

# Get execution details
aws stepfunctions describe-execution `
    --execution-arn <EXECUTION_ARN> `
    --region eu-central-1
```

**Execution Verification Checklist**:
- [ ] All successful tests show `Status: SUCCEEDED`
- [ ] Failed tests show `Status: SUCCEEDED` (graceful error handling)
- [ ] Error paths executed for validation failures
- [ ] X-Ray traces available
- [ ] Visual workflow shows correct path

---

### DynamoDB Job Status Verification

Query job status:

```powershell
# Scan all jobs
aws dynamodb scan --table-name InvestTax-Jobs-dev --region eu-central-1

# Get specific job
aws dynamodb get-item `
    --table-name InvestTax-Jobs-dev `
    --key '{"jobId":{"S":"YOUR_JOB_ID"}}' `
    --region eu-central-1
```

**DynamoDB Verification Checklist**:
- [ ] Job records created for all tests
- [ ] Status correctly set (`Completed` or `Failed`)
- [ ] Timestamps recorded (startTime, endTime)
- [ ] Error messages present for failed jobs
- [ ] S3 keys stored for input/output files

---

### Email Verification

Check test email inbox:

**Success Email Checklist**:
- [ ] Email received within 5 minutes of upload
- [ ] Subject: "InvestTax Calculator - Tax Report 2024"
- [ ] Report content included in email body
- [ ] HTML formatting correct
- [ ] Summary section displays key metrics
- [ ] Detailed transactions listed
- [ ] Job ID referenced

**Error Email Checklist**:
- [ ] Email received within 2 minutes of upload
- [ ] Subject: "InvestTax Calculator - Processing Failed"
- [ ] Error details clearly explained
- [ ] Actionable guidance provided
- [ ] Support contact information included

---

### Tax Calculation Validation

For successful tests, validate tax calculations:

**Test 1 Expected Calculation**:
```
Buy:  10 shares × $150 USD × 3.95 PLN/USD = 5,925 PLN
Sell: 10 shares × $180 USD × 4.10 PLN/USD = 7,380 PLN
Gain: 7,380 - 5,925 = 1,455 PLN
Tax:  1,455 × 19% = 276.45 PLN
```

**Validation Steps**:
1. Download generated report from S3
2. Parse summary section
3. Compare total gain PLN against expected
4. Compare tax PLN against expected (19% of gain)
5. Verify individual transaction breakdowns
6. Check exchange rates used match NBP API data

---

## Performance Benchmarks

### Target Performance Metrics

| Metric | Target | Test Result | Pass/Fail |
|--------|--------|-------------|-----------|
| **Overall Execution Time** | < 5 minutes | ___ minutes | ___ |
| **Validation Stage** | < 30 seconds | ___ seconds | ___ |
| **Normalization Stage** | < 45 seconds | ___ seconds | ___ |
| **NBP Rate Fetch** | < 60 seconds | ___ seconds | ___ |
| **Tax Calculation** | < 30 seconds | ___ seconds | ___ |
| **Report Generation** | < 20 seconds | ___ seconds | ___ |
| **Email Delivery** | < 10 seconds | ___ seconds | ___ |

### Monitoring Performance

Extract execution duration from Step Functions:

```powershell
# Get execution history for timing
aws stepfunctions get-execution-history `
    --execution-arn <EXECUTION_ARN> `
    --region eu-central-1 `
    --output json | ConvertFrom-Json | ForEach-Object { $_.events }
```

---

## Post-Test Cleanup

### Clean Up Test Data

Remove test files from S3:

```powershell
# List test uploads
aws s3 ls s3://investtax-upload-dev/your-test-email@example.com/ --region eu-central-1

# Delete test files (optional)
aws s3 rm s3://investtax-upload-dev/your-test-email@example.com/ --recursive --region eu-central-1

# Clean processing bucket
aws s3 ls s3://investtax-processing-dev/ --region eu-central-1
# Review and delete old test artifacts as needed
```

### Clean Up DynamoDB Records

```powershell
# List test jobs
aws dynamodb scan --table-name InvestTax-Jobs-dev --region eu-central-1

# Delete specific job (optional - for test environment only)
aws dynamodb delete-item `
    --table-name InvestTax-Jobs-dev `
    --key '{"jobId":{"S":"YOUR_TEST_JOB_ID"}}' `
    --region eu-central-1
```

### Archive Test Results

Save test results and reports:

```powershell
# Create results directory
mkdir test-results\$(Get-Date -Format 'yyyyMMdd')

# Download all generated reports
aws s3 cp s3://investtax-processing-dev/reports/ ./test-results/$(Get-Date -Format 'yyyyMMdd')/ --recursive --region eu-central-1

# Save test execution JSON
# (Already saved by run-integration-tests.ps1)
```

---

## Test Sign-Off

### Test Execution Summary

**Date**: _______________  
**Tester**: _______________  
**Environment**: Development (dev)  

**Results Summary**:
- Total Tests: 5
- Passed: ___ / 5
- Failed: ___ / 5
- Performance: All tests < 5 minutes: Yes / No

**Issues Identified**:
1. _______________________________________________
2. _______________________________________________
3. _______________________________________________

**Recommendations**:
1. _______________________________________________
2. _______________________________________________
3. _______________________________________________

**Sign-Off**: 
- [ ] All critical tests passed
- [ ] Performance benchmarks met
- [ ] Error handling verified
- [ ] Documentation complete
- [ ] Ready for production deployment

**Signature**: _______________  
**Date**: _______________

---

## Troubleshooting Common Issues

### Issue: State Machine Not Triggered

**Symptoms**: File uploaded but no Step Functions execution  
**Possible Causes**:
- EventBridge notifications not enabled on S3 bucket
- EventBridge rule misconfigured
- IAM permissions missing

**Resolution**:
```powershell
# Check EventBridge rules
aws events list-rules --region eu-central-1 | Select-String "investtax"

# Verify S3 event configuration
aws s3api get-bucket-notification-configuration --bucket investtax-upload-dev --region eu-central-1
```

---

### Issue: Validation Errors Not Caught

**Symptoms**: Invalid data passed validation stage  
**Resolution**: Review ValidationService logic and FluentValidation rules

---

### Issue: NBP API Failures

**Symptoms**: Rate fetch Lambda times out or fails  
**Possible Causes**:
- NBP API unavailable (weekends/holidays)
- Network connectivity issues
- Rate limiting

**Resolution**:
- Check Polly retry logs
- Verify NBP API endpoint: https://api.nbp.pl/api/
- Implement weekend/holiday fallback logic

---

### Issue: Email Not Received

**Symptoms**: Workflow completes but no email  
**Possible Causes**:
- SES sender not verified
- SES in sandbox mode
- Recipient email not verified (sandbox)
- Email in spam folder

**Resolution**:
```powershell
# Verify SES configuration
aws ses get-account-sending-enabled --region eu-central-1
aws ses list-verified-email-addresses --region eu-central-1

# Check SES sent emails
aws ses list-identity-verification-attributes --region eu-central-1
```

---

## Next Steps After Successful Testing

1. **Performance Optimization** (Phase 2):
   - Implement DynamoDB rate caching
   - Enable parallel NBP API calls
   - Add HTML report generation

2. **Security Hardening**:
   - Review IAM least privilege policies
   - Enable S3 bucket encryption
   - Configure VPC for Lambda functions

3. **Monitoring Enhancement**:
   - Set up CloudWatch Alarms
   - Create operational dashboard
   - Configure SNS alerts for failures

4. **Production Deployment**:
   - Deploy to production environment
   - Update DNS for custom domain
   - Scale testing with larger files
   - Load testing (multiple concurrent uploads)

---

**End of Test Checklist**
