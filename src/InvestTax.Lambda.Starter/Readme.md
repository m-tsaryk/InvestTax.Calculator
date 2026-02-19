# InvestTax.Lambda.Starter

## Overview

This Lambda function is triggered by S3 ObjectCreated events when a CSV file is uploaded to the upload bucket. It performs the following tasks:

1. **Extracts metadata** from the S3 event (bucket name, object key)
2. **Validates the S3 key format** to extract user email (expected format: `email@example.com/filename.csv`)
3. **Creates a job record** in DynamoDB with initial status
4. **Starts the Step Functions workflow** to process the CSV file

## Environment Variables

- `STATE_MACHINE_ARN` - ARN of the Step Functions state machine to invoke
- `JOBS_TABLE` - Name of the DynamoDB table for job tracking
- `PROCESSING_BUCKET` - Name of the S3 bucket for intermediate processing files

## S3 Key Format

CSV files should be uploaded with the following key format:
```
email@example.com/filename.csv
```

The email address will be extracted from the key and used to:
- Create the job record in DynamoDB
- Send notification emails at the end of processing

## Testing Locally

### Using AWS SAM CLI

```bash
# Create test event
cat > test-event.json << EOF
{
  "Records": [
    {
      "s3": {
        "bucket": {
          "name": "investtax-upload-dev"
        },
        "object": {
          "key": "test@example.com/transactions.csv"
        }
      }
    }
  ]
}
EOF

# Invoke function
sam local invoke StarterFunction -e test-event.json
```

### Using dotnet-lambda CLI

```powershell
# Install tool
dotnet tool install -g Amazon.Lambda.Tools

# Test function
dotnet lambda invoke-function InvestTax-Starter-dev --payload test-event.json
```

## Deployment

The function is deployed as part of the CDK stack in `infrastructure/cdk/lib/investtax-stack.ts`.

```bash
cd infrastructure/cdk
cdk deploy InvestTaxStack-dev
```

## IAM Permissions Required

- `states:StartExecution` - Start Step Functions execution
- `dynamodb:PutItem` - Create job record in DynamoDB
- `logs:CreateLogGroup` - Create CloudWatch log group
- `logs:CreateLogStream` - Create CloudWatch log stream
- `logs:PutLogEvents` - Write CloudWatch logs

## Error Handling

- Invalid S3 key format: Logged as warning, skips processing
- Invalid email format: Logged as warning, skips processing
- DynamoDB errors: Thrown and logged, will cause Lambda invocation to fail
- Step Functions errors: Thrown and logged, will cause Lambda invocation to fail

## Monitoring

CloudWatch metrics:
- `Invocations` - Number of times function is invoked
- `Errors` - Number of failed invocations
- `Duration` - Execution time
- `Throttles` - Number of throttled invocations

CloudWatch logs: `/aws/lambda/InvestTax-Starter-{stage}`
