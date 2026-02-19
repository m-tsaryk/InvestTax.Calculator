# InvestTax Calculator - Local Development Guide

Complete guide for running the entire InvestTax application locally using Docker containers and LocalStack.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Architecture Overview](#architecture-overview)
- [Complete Deployment](#complete-deployment)
- [Available Scripts](#available-scripts)
- [Testing Workflow](#testing-workflow)
- [Monitoring & Debugging](#monitoring--debugging)
- [Troubleshooting](#troubleshooting)
- [Daily Development Workflow](#daily-development-workflow)
- [Performance Notes](#performance-notes)
- [Resources](#resources)

## Prerequisites

Before starting, ensure you have:

- ✅ **Docker Desktop** (running)
- ✅ **AWS CLI v2**
- ✅ **PowerShell 5.1+**
- ✅ **.NET SDK 10.0**
- ✅ **Node.js** (for CDK infrastructure)

**Verify all prerequisites:**
```powershell
.\scripts\verify-local.ps1
```

## Quick Start

**Complete one-command deployment:**
```powershell
# Deploy everything (first time: 10-15 minutes)
.\scripts\deploy-local-complete.ps1

# Wait for completion, then test
.\scripts\test-upload.ps1

# Verify results
.\scripts\verify-workflow.ps1

# View data in DynamoDB UI
Start-Process "http://localhost:8001"
```

## Architecture Overview

The local deployment includes:

- **7 Lambda Functions** (containerized .NET 10.0)
  - Validator, Normalizer, NBPClient, Calculator, ReportGenerator, EmailSender, Starter
- **Step Functions State Machine** (workflow orchestration)
- **S3 Buckets** (investtax-upload-local, investtax-processing-local)
- **DynamoDB Table** (InvestTax-Jobs-Local)
- **SES** (email simulation)

## Complete Deployment

### Option 1: One-Command Deployment (Recommended)

Deploy everything with a single command:

```powershell
.\scripts\deploy-local-complete.ps1
```

**What it does:**
1. Starts LocalStack (if not running)
2. Builds all Lambda Docker images (~5-10 minutes first time)
3. Deploys Lambda functions to LocalStack
4. Creates Step Functions state machine
5. Configures S3 event notifications

**⏱️ First-time deployment: 10-15 minutes** (Docker image building with caching)

**Subsequent deployments: 3-5 minutes** (cached Docker layers)

### Option 2: Step-by-Step Deployment

For manual control or troubleshooting:

#### Step 1: Start LocalStack
```powershell
.\scripts\start-local.ps1
```
Wait ~30 seconds for LocalStack to be ready.

#### Step 2: Build Lambda Docker Images
```powershell
.\scripts\build-lambda-images.ps1
```
Builds Docker containers for all 7 Lambda functions (~5-10 minutes).

**Rebuild without cache:**
```powershell
.\scripts\build-lambda-images.ps1 -NoBuildCache
```

#### Step 3: Deploy Lambda Functions
```powershell
.\scripts\deploy-lambdas-local.ps1
```
Deploys containerized Lambda functions to LocalStack (~2 minutes).

#### Step 4: Deploy Step Functions
```powershell
.\scripts\deploy-stepfunctions-local.ps1
```
Creates workflow state machine and S3 event notifications (~30 seconds).

## Available Scripts

### Infrastructure Management Scripts

#### start-local.ps1

Starts the LocalStack environment and initializes AWS resources (S3, DynamoDB, SES).

**Usage:**
```powershell
.\scripts\start-local.ps1
```

**Options:**
- `-SkipInit` - Skip AWS resource initialization (faster restart)

**Examples:**
```powershell
# Full initialization (first time)
.\scripts\start-local.ps1

# Quick restart (skip resource creation)
.\scripts\start-local.ps1 -SkipInit
```

**What it initializes:**
- S3 buckets: investtax-upload-local, investtax-processing-local
- DynamoDB table: InvestTax-Jobs-Local
- SES verified email: test@example.com

#### stop-local.ps1

Stops the LocalStack environment.

**Usage:**
```powershell
.\scripts\stop-local.ps1
```

**Options:**
- `-RemoveVolumes` - Remove all data volumes (complete cleanup)

**Examples:**
```powershell
# Stop but keep data
.\scripts\stop-local.ps1

# Stop and remove all data (fresh start)
.\scripts\stop-local.ps1 -RemoveVolumes
```

#### check-local.ps1

Checks the status of the local development environment.

**Usage:**
```powershell
.\scripts\check-local.ps1
```

**Displays:**
- Running Docker containers
- LocalStack service health
- Available S3 buckets
- DynamoDB tables
- Service URLs

#### verify-local.ps1

Comprehensive verification of all prerequisites and environment setup.

**Usage:**
```powershell
.\scripts\verify-local.ps1
```

**Checks:**
- Docker installation and status
- AWS CLI version
- Node.js version
- .NET SDK version
- Running containers
- LocalStack health

### Deployment Scripts

#### deploy-local-complete.ps1

**One-command complete deployment** - Builds and deploys everything.

**Usage:**
```powershell
.\scripts\deploy-local-complete.ps1
```

**Options:**
- `-SkipBuild` - Skip Docker image building (use existing images)
- `-SkipInfrastructure` - Skip LocalStack startup (assume running)

**Examples:**
```powershell
# Full deployment
.\scripts\deploy-local-complete.ps1

# Redeploy without rebuilding images
.\scripts\deploy-local-complete.ps1 -SkipBuild

# Deploy assuming LocalStack is already running
.\scripts\deploy-local-complete.ps1 -SkipInfrastructure
```

#### build-lambda-images.ps1

Builds Docker images for all Lambda functions.

**Usage:**
```powershell
.\scripts\build-lambda-images.ps1
```

**Options:**
- `-NoBuildCache` - Force rebuild without Docker cache

**Examples:**
```powershell
# Build with cache (faster)
.\scripts\build-lambda-images.ps1

# Clean build without cache
.\scripts\build-lambda-images.ps1 -NoBuildCache
```

**Builds 7 images:**
- investtax-lambda-validator
- investtax-lambda-normalizer
- investtax-lambda-nbpclient
- investtax-lambda-calculator
- investtax-lambda-reportgenerator
- investtax-lambda-emailsender
- investtax-lambda-starter

#### deploy-lambdas-local.ps1

Deploys containerized Lambda functions to LocalStack.

**Usage:**
```powershell
.\scripts\deploy-lambdas-local.ps1
```

**What it does:**
- Deletes existing Lambda functions (if any)
- Creates new Lambda functions from Docker images
- Configures environment variables
- Sets memory, timeout, and IAM roles

#### deploy-stepfunctions-local.ps1

Deploys Step Functions state machine and S3 event notifications.

**Usage:**
```powershell
.\scripts\deploy-stepfunctions-local.ps1
```

**What it creates:**
- Step Functions state machine: InvestTax-Workflow-Local
- S3 event notification: CSV uploads → Starter Lambda
- IAM role for Step Functions

### Testing & Verification Scripts

#### test-upload.ps1

Uploads a CSV file to S3 to trigger the complete workflow.

**Usage:**
```powershell
.\scripts\test-upload.ps1
```

**Options:**
- `-BucketName` - Target S3 bucket (default: auto-detected based on -Local)
- `-Email` - User email address (default: `test@example.com`)
- `-CsvFile` - Path to CSV file (default: `test-data\sample.csv`)
- `-Region` - AWS region (default: `eu-central-1`)
- `-Profile` - AWS CLI profile (optional, for real AWS)
- `-Local` - Use LocalStack (default: `$true`)

**Examples:**
```powershell
# Upload to LocalStack (default)
.\scripts\test-upload.ps1

# Upload specific test file
.\scripts\test-upload.ps1 -CsvFile "test-data\test-1-simple-success.csv"

# Upload to real AWS
.\scripts\test-upload.ps1 -Local:$false -Profile "my-aws-profile"

# Upload with different email
.\scripts\test-upload.ps1 -Email "john@example.com" -CsvFile "test-data\test-2-multiple-stocks.csv"
```

**What it does:**
1. Validates CSV file exists
2. Generates unique timestamped filename
3. Uploads to S3: `email@example.com/filename-timestamp.csv`
4. Triggers Starter Lambda (if S3 events configured)
5. Displays upload confirmation and monitoring info

**Workflow triggered:**
1. Starter Lambda receives S3 event
2. Job record created in DynamoDB
3. Step Functions workflow starts
4. CSV validation
5. Data normalization
6. Exchange rate fetching (NBP API)
7. Tax calculation
8. Report generation (PDF)
9. Email delivery (SES)

#### verify-workflow.ps1

Checks the status of uploaded files and workflow results.

**Usage:**
```powershell
.\scripts\verify-workflow.ps1
```

**Options:**
- `-Email` - Filter by email (default: `test@example.com`)
- `-JobId` - Check specific job by ID
- `-Local` - Check LocalStack (default: `$true`)

**Examples:**
```powershell
# Check recent jobs for default email
.\scripts\verify-workflow.ps1

# Check specific job
.\scripts\verify-workflow.ps1 -JobId "12345678-1234-1234-1234-123456789abc"

# Check different email
.\scripts\verify-workflow.ps1 -Email "john@example.com"

# Check real AWS
.\scripts\verify-workflow.ps1 -Local:$false
```

**What it checks:**
1. DynamoDB job records and status
2. Uploaded files in S3 upload bucket
3. Generated reports in S3 processing bucket
4. Email logs (LocalStack only)

#### run-integration-tests.ps1

Runs all integration test scenarios from the test-data folder.

**Usage:**
```powershell
.\scripts\run-integration-tests.ps1
```

**Options:**
- `-BucketName` - Target bucket
- `-Email` - Test email address
- `-Region` - AWS region
- `-StateMachineArn` - Step Functions ARN
- `-TimeoutSeconds` - Timeout per test (default: 300)
- `-SkipValidation` - Skip result validation

**Test scenarios:**
- test-1-simple-success.csv - Basic buy/sell transaction
- test-2-multiple-stocks.csv - Multiple ISINs
- test-3-partial-fills.csv - Complex FIFO scenario
- test-4-validation-error-missing-isin.csv - Validation error
- test-5-validation-error-invalid-date.csv - Date validation error

## Testing Workflow

### Complete Test Flow

```powershell
# 1. Deploy everything (one-time or after changes)
.\scripts\deploy-local-complete.ps1

# 2. Upload test file
.\scripts\test-upload.ps1 -CsvFile "test-data\test-1-simple-success.csv"

# 3. Wait 10-30 seconds for processing

# 4. Verify results
.\scripts\verify-workflow.ps1

# 5. View in DynamoDB Admin UI
Start-Process "http://localhost:8001"
```

### View Job Details in DynamoDB

**Open DynamoDB Admin:**
```powershell
Start-Process "http://localhost:8001"
```

Then:
1. Select table: `InvestTax-Jobs-Local`
2. View job records with status, timestamps, errors
3. See report S3 keys

### Download Generated Report

```powershell
# List all reports
aws --endpoint-url=http://localhost:4566 s3 ls s3://investtax-processing-local/reports/ --recursive

# List reports for specific email
aws --endpoint-url=http://localhost:4566 s3 ls s3://investtax-processing-local/reports/test@example.com/

# Download specific report
aws --endpoint-url=http://localhost:4566 s3 cp s3://investtax-processing-local/reports/test@example.com/report-20260219.pdf ./my-report.pdf
```

## Monitoring & Debugging

### Check LocalStack Logs

```powershell
# Follow all LocalStack logs
docker logs investtax-calculator-localstack-1 -f

# Search for specific Lambda
docker logs investtax-calculator-localstack-1 2>&1 | Select-String -Pattern "InvestTax-Validator"

# Check for errors
docker logs investtax-calculator-localstack-1 2>&1 | Select-String -Pattern "ERROR|Exception"
```

### Check Lambda Function Logs

```powershell
# List log groups
aws --endpoint-url=http://localhost:4566 logs describe-log-groups --region eu-central-1

# Tail logs for specific Lambda
aws --endpoint-url=http://localhost:4566 logs tail /aws/lambda/InvestTax-Validator-Local --follow --region eu-central-1
```

### List Deployed Resources

**Lambda Functions:**
```powershell
aws --endpoint-url=http://localhost:4566 lambda list-functions --region eu-central-1 --query 'Functions[?contains(FunctionName, `InvestTax`)].FunctionName' --output table
```

**Step Functions:**
```powershell
aws --endpoint-url=http://localhost:4566 stepfunctions list-state-machines --region eu-central-1
```

**Recent Workflow Executions:**
```powershell
aws --endpoint-url=http://localhost:4566 stepfunctions list-executions --state-machine-arn "arn:aws:states:eu-central-1:000000000000:stateMachine:InvestTax-Workflow-Local" --region eu-central-1
```

**Execution Details:**
```powershell
aws --endpoint-url=http://localhost:4566 stepfunctions describe-execution --execution-arn "<execution-arn>" --region eu-central-1
```

**S3 Buckets and Contents:**
```powershell
# List buckets
aws --endpoint-url=http://localhost:4566 s3 ls

# List files in upload bucket
aws --endpoint-url=http://localhost:4566 s3 ls s3://investtax-upload-local --recursive

# List files in processing bucket
aws --endpoint-url=http://localhost:4566 s3 ls s3://investtax-processing-local --recursive
```

**DynamoDB Table Scan:**
```powershell
# Scan all jobs
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name InvestTax-Jobs-Local --region eu-central-1

# Get specific job
aws --endpoint-url=http://localhost:4566 dynamodb get-item --table-name InvestTax-Jobs-Local --key '{"JobId":{"S":"your-job-id"}}' --region eu-central-1
```

## Troubleshooting

### Docker Build Fails

**Solution:**
```powershell
# Clean Docker cache and rebuild
docker system prune -a -f
.\scripts\build-lambda-images.ps1 -NoBuildCache
```

### Lambda Deployment Fails

**Solution:**
```powershell
# Restart LocalStack and redeploy
.\scripts\stop-local.ps1
.\scripts\start-local.ps1
.\scripts\deploy-lambdas-local.ps1
```

### No Jobs Appear in DynamoDB

**Possible causes:**
1. S3 event notification not triggered
2. Starter Lambda not invoked
3. Step Functions not started
4. Lambda function errors

**Solution:**
```powershell
# Check Lambda logs
docker logs investtax-calculator-localstack-1 2>&1 | Select-String -Pattern "InvestTax-Starter"

# Manually invoke Starter Lambda
$payload = @'
{
  "Records": [{
    "s3": {
      "bucket": {"name": "investtax-upload-local"},
      "object": {"key": "test@example.com/test.csv"}
    }
  }]
}
'@

aws --endpoint-url=http://localhost:4566 lambda invoke --function-name InvestTax-Starter-Local --payload $payload response.json --region eu-central-1

# Check response
Get-Content response.json
```

### Workflow Fails at Specific Step

**Solution:**
```powershell
# Get execution details
aws --endpoint-url=http://localhost:4566 stepfunctions describe-execution --execution-arn "<execution-arn>" --region eu-central-1

# Check specific Lambda logs
aws --endpoint-url=http://localhost:4566 logs tail /aws/lambda/InvestTax-Validator-Local --region eu-central-1

# Check DynamoDB for error messages
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name InvestTax-Jobs-Local --region eu-central-1 --filter-expression "attribute_exists(ErrorMessage)"
```

### LocalStack Not Responding

**Solution:**
```powershell
# Check Docker status
docker ps

# Restart LocalStack
.\scripts\stop-local.ps1
.\scripts\start-local.ps1

# Check health
Invoke-RestMethod -Uri "http://localhost:4566/_localstack/health"
```

### Port Conflicts

**Error:** `Port 4566 or 8001 already in use`

**Solution:**
```powershell
# Find what's using the port
Get-NetTCPConnection -LocalPort 4566

# Stop the conflicting service or change docker-compose.yml ports
```

### AWS CLI Not Found

**Solution:**
Download and install AWS CLI v2:
https://awscli.amazonaws.com/AWSCLIV2.msi

### .NET SDK Issues

**Solution:**
```powershell
# Verify .NET SDK
dotnet --list-sdks

# If .NET 10.0 not found, download from:
# https://dotnet.microsoft.com/download/dotnet/10.0
```

## Daily Development Workflow

### Starting Work

```powershell
# Quick start (if already deployed)
.\scripts\start-local.ps1 -SkipInit

# Check status
.\scripts\check-local.ps1
```

### After Code Changes

```powershell
# Rebuild and redeploy Lambdas only (faster)
.\scripts\build-lambda-images.ps1
.\scripts\deploy-lambdas-local.ps1

# Test changes
.\scripts\test-upload.ps1
.\scripts\verify-workflow.ps1
```

### Ending Work

```powershell
# Stop but keep data for tomorrow
.\scripts\stop-local.ps1
```

### Complete Reset

```powershell
# When you need a fresh start
.\scripts\stop-local.ps1 -RemoveVolumes
.\scripts\deploy-local-complete.ps1
```

### Redeployment Scenarios

**After Lambda code changes:**
```powershell
.\scripts\build-lambda-images.ps1
.\scripts\deploy-lambdas-local.ps1
```

**After Step Functions definition changes:**
```powershell
.\scripts\deploy-stepfunctions-local.ps1
```

**Complete fresh deployment:**
```powershell
.\scripts\stop-local.ps1 -RemoveVolumes
.\scripts\deploy-local-complete.ps1
```

## Performance Notes

- **First build:** 10-15 minutes (Docker images from scratch)
- **Subsequent builds:** 3-5 minutes (cached Docker layers)
- **Lambda deployment:** 1-2 minutes (7 functions)
- **Step Functions deployment:** 30 seconds
- **Workflow execution:** 10-30 seconds per CSV file

**Tips for faster iterations:**
- Use `-SkipBuild` when Lambda code hasn't changed
- Use `-SkipInit` when restarting LocalStack
- Keep Docker Desktop running
- Clean up old Docker images periodically

## Manual AWS CLI Commands

### S3 Operations

```powershell
# List buckets
aws --endpoint-url=http://localhost:4566 s3 ls

# List files in bucket
aws --endpoint-url=http://localhost:4566 s3 ls s3://investtax-upload-local/test@example.com/

# Upload file
aws --endpoint-url=http://localhost:4566 s3 cp test.csv s3://investtax-upload-local/test@example.com/ --region eu-central-1

# Download file
aws --endpoint-url=http://localhost:4566 s3 cp s3://investtax-processing-local/reports/test@example.com/report.pdf ./report.pdf --region eu-central-1

# Delete file
aws --endpoint-url=http://localhost:4566 s3 rm s3://investtax-upload-local/test@example.com/test.csv --region eu-central-1
```

### DynamoDB Operations

```powershell
# List tables
aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-central-1

# Scan table  
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name InvestTax-Jobs-Local --region eu-central-1

# Get item
aws --endpoint-url=http://localhost:4566 dynamodb get-item --table-name InvestTax-Jobs-Local --key '{"JobId":{"S":"abc-123"}}' --region eu-central-1

# Query by status (using GSI)
aws --endpoint-url=http://localhost:4566 dynamodb query --table-name InvestTax-Jobs-Local --index-name StatusIndex --key-condition-expression "Status = :status" --expression-attribute-values '{":status":{"S":"COMPLETED"}}' --region eu-central-1
```

### Lambda Operations

```powershell
# List functions
aws --endpoint-url=http://localhost:4566 lambda list-functions --region eu-central-1

# Invoke function
aws --endpoint-url=http://localhost:4566 lambda invoke --function-name InvestTax-Validator-Local --payload '{"test":"data"}' response.json --region eu-central-1

# Get function configuration
aws --endpoint-url=http://localhost:4566 lambda get-function --function-name InvestTax-Validator-Local --region eu-central-1

# Update environment variables
aws --endpoint-url=http://localhost:4566 lambda update-function-configuration --function-name InvestTax-Validator-Local --environment "Variables={KEY=VALUE}" --region eu-central-1
```

### Step Functions Operations

```powershell
# List state machines
aws --endpoint-url=http://localhost:4566 stepfunctions list-state-machines --region eu-central-1

# Start execution
aws --endpoint-url=http://localhost:4566 stepfunctions start-execution --state-machine-arn "arn:aws:states:eu-central-1:000000000000:stateMachine:InvestTax-Workflow-Local" --input '{"JobId":"test-123","Email":"test@example.com"}' --region eu-central-1

# List executions
aws --endpoint-url=http://localhost:4566 stepfunctions list-executions --state-machine-arn "arn:aws:states:eu-central-1:000000000000:stateMachine:InvestTax-Workflow-Local" --region eu-central-1

# Describe execution
aws --endpoint-url=http://localhost:4566 stepfunctions describe-execution --execution-arn "<arn>" --region eu-central-1

# Get execution history
aws --endpoint-url=http://localhost:4566 stepfunctions get-execution-history --execution-arn "<arn>" --region eu-central-1
```

## Resources

### Quick Links

- **DynamoDB Admin:** http://localhost:8001
- **LocalStack Health:** http://localhost:4566/_localstack/health  
- **LocalStack Gateway:** http://localhost:4566

### Test Data

Sample test files in `test-data/`:
- `test-1-simple-success.csv` - Basic successful transaction
- `test-2-multiple-stocks.csv` - Multiple ISINs with FIFO
- `test-3-partial-fills.csv` - Complex FIFO scenario
- `test-4-validation-error-missing-isin.csv` - Validation error
- `test-5-validation-error-invalid-date.csv` - Date validation error

See `test-data/SCENARIOS.md` for detailed descriptions.

### Docker Images

Built images for Lambda functions:
- `investtax-lambda-validator:latest`
- `investtax-lambda-normalizer:latest`
- `investtax-lambda-nbpclient:latest`
- `investtax-lambda-calculator:latest`
- `investtax-lambda-reportgenerator:latest`
- `investtax-lambda-emailsender:latest`
- `investtax-lambda-starter:latest`

**Manage images:**
```powershell
# List images
docker images | Select-String "investtax-lambda"

# Remove all InvestTax images
docker images | Select-String "investtax-lambda" | ForEach-Object { $_.ToString().Split()[0] } | ForEach-Object { docker rmi $_ -f }

# Remove unused images
docker image prune -a
```

### Next Steps

After successful local deployment:

**1. Run comprehensive tests:**
```powershell
.\scripts\run-integration-tests.ps1
```

**2. Run .NET unit tests:**
```powershell
dotnet test ..\InvestTax.Calculator.sln
```

**3. Deploy to AWS dev environment:**
```powershell
.\scripts\deploy.ps1 -Stage dev
```

**4. Deploy to AWS production:**
```powershell
.\scripts\deploy.ps1 -Stage prod
```

### Documentation

- LocalStack: https://docs.localstack.cloud/
- AWS CLI: https://docs.aws.amazon.com/cli/
- Docker: https://docs.docker.com/
- AWS Lambda: https://docs.aws.amazon.com/lambda/
- Step Functions: https://docs.aws.amazon.com/step-functions/

### Windows-Specific Notes

- Scripts are designed for **PowerShell on Windows**
- Bash versions (`.sh`) available in `infrastructure/` for Git Bash
- Use **PowerShell** (not Command Prompt)
- Enable script execution if needed:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

### Support

For issues or questions:

1. **Check LocalStack logs:**
   ```powershell
   docker logs investtax-calculator-localstack-1
   ```

2. **Verify environment:**
   ```powershell
   .\scripts\verify-local.ps1
   .\scripts\check-local.ps1
   ```

3. **Review test scenarios:**
   ```
   test-data\SCENARIOS.md
   test-data\TEST-CHECKLIST.md
   ```

4. **Check documentation:**
   ```
   docs\architecture\README.md
   ```

---

**Happy coding! 🚀**
