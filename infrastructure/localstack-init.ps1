# LocalStack Initialization Script for Windows
# This script sets up S3 buckets, DynamoDB tables, and SES for local development

Write-Host "Waiting for LocalStack to be ready..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

# Set AWS endpoint configuration
$env:AWS_ENDPOINT = "http://localhost:4566"
$env:AWS_REGION = "eu-central-1"
$env:AWS_ACCESS_KEY_ID = "test"
$env:AWS_SECRET_ACCESS_KEY = "test"

$endpoint = "http://localhost:4566"
$region = "eu-central-1"

# Create S3 buckets
Write-Host "`nCreating S3 buckets..." -ForegroundColor Cyan
aws --endpoint-url=$endpoint s3 mb s3://investtax-upload-local
aws --endpoint-url=$endpoint s3 mb s3://investtax-processing-local

# Create DynamoDB table
Write-Host "`nCreating DynamoDB Jobs table..." -ForegroundColor Cyan

$gsiJson = @'
[
  {
    "IndexName": "StatusIndex",
    "KeySchema": [
      {
        "AttributeName": "Status",
        "KeyType": "HASH"
      }
    ],
    "Projection": {
      "ProjectionType": "ALL"
    },
    "ProvisionedThroughput": {
      "ReadCapacityUnits": 5,
      "WriteCapacityUnits": 5
    }
  }
]
'@

aws --endpoint-url=$endpoint dynamodb create-table `
  --table-name InvestTax-Jobs-Local `
  --attribute-definitions `
    AttributeName=JobId,AttributeType=S `
    AttributeName=Status,AttributeType=S `
  --key-schema `
    AttributeName=JobId,KeyType=HASH `
  --global-secondary-indexes $gsiJson `
  --billing-mode PAY_PER_REQUEST `
  --region $region

# Verify SES email
Write-Host "`nVerifying SES email for testing..." -ForegroundColor Cyan
aws --endpoint-url=$endpoint ses verify-email-identity `
  --email-address test@example.com `
  --region $region

Write-Host "`nLocalStack initialization complete!" -ForegroundColor Green
Write-Host "`nAvailable resources:" -ForegroundColor Yellow
Write-Host "  - S3 Buckets: investtax-upload-local, investtax-processing-local"
Write-Host "  - DynamoDB Table: InvestTax-Jobs-Local"
Write-Host "  - SES Verified Email: test@example.com"
Write-Host "  - DynamoDB Admin UI: http://localhost:8001`n"
