#!/bin/bash

echo "Waiting for LocalStack to be ready..."
sleep 5

# Set AWS endpoint
export AWS_ENDPOINT=http://localhost:4566
export AWS_REGION=eu-central-1
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test

# Create S3 buckets
echo "Creating S3 buckets..."
aws --endpoint-url=$AWS_ENDPOINT s3 mb s3://investtax-upload-local
aws --endpoint-url=$AWS_ENDPOINT s3 mb s3://investtax-processing-local

# Create DynamoDB table
echo "Creating DynamoDB Jobs table..."
aws --endpoint-url=$AWS_ENDPOINT dynamodb create-table \
  --table-name InvestTax-Jobs-Local \
  --attribute-definitions \
    AttributeName=JobId,AttributeType=S \
    AttributeName=Status,AttributeType=S \
  --key-schema \
    AttributeName=JobId,KeyType=HASH \
  --global-secondary-indexes \
    "[{\"IndexName\":\"StatusIndex\",\"KeySchema\":[{\"AttributeName\":\"Status\",\"KeyType\":\"HASH\"}],\"Projection\":{\"ProjectionType\":\"ALL\"},\"ProvisionedThroughput\":{\"ReadCapacityUnits\":5,\"WriteCapacityUnits\":5}}]" \
  --billing-mode PAY_PER_REQUEST \
  --region $AWS_REGION

# Verify SES email
echo "Verifying SES email for testing..."
aws --endpoint-url=$AWS_ENDPOINT ses verify-email-identity \
  --email-address test@example.com \
  --region $AWS_REGION

echo "LocalStack initialization complete!"
echo ""
echo "Available resources:"
echo "  - S3 Buckets: investtax-upload-local, investtax-processing-local"
echo "  - DynamoDB Table: InvestTax-Jobs-Local"
echo "  - SES Verified Email: test@example.com"
echo "  - DynamoDB Admin UI: http://localhost:8001"
