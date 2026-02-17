# InvestTax Calculator - CDK Infrastructure

This directory contains the AWS CDK infrastructure code for the InvestTax Calculator application.

## Prerequisites

- Node.js 20.x LTS
- AWS CLI configured with credentials
- TypeScript 5.3.x

## Installation

```bash
npm install
```

## Build

```bash
npm run build
```

## CDK Commands

### Synthesize CloudFormation template

```bash
# For development environment
npx cdk synth -c stage=dev

# For production environment
npx cdk synth -c stage=prod
```

### Deploy

```bash
# Deploy to development
npx cdk deploy -c stage=dev

# Deploy to production
npx cdk deploy -c stage=prod
```

### Bootstrap (first-time setup)

```bash
npx cdk bootstrap aws://ACCOUNT-ID/eu-central-1
```

### View differences

```bash
npx cdk diff -c stage=dev
```

### Destroy stack

```bash
npx cdk destroy -c stage=dev
```

## Infrastructure Overview

The CDK stack provisions the following AWS resources:

### Storage
- **S3 Buckets**:
  - `investtax-upload-{stage}`: User file uploads (30-day lifecycle)
  - `investtax-processing-{stage}`: Intermediate processing files (7-day lifecycle)

### Database
- **DynamoDB Table**:
  - `InvestTax-Jobs-{stage}`: Job tracking with StatusIndex GSI

### Compute
- **Lambda Functions**:
  - Validator: CSV validation
  - Normalizer: Data normalization
  - NBPClient: Exchange rate fetching
  - Calculator: Tax calculation (FIFO)
  - ReportGenerator: Report generation
  - EmailSender: Email delivery
  - Trigger: S3 event handler

### Orchestration
- **Step Functions State Machine**:
  - Sequential workflow orchestrating all Lambda functions
  - Error handling and logging

### Notifications
- **S3 Event Notification**:
  - Triggers workflow on CSV upload

## Environment Variables

Lambda functions receive the following environment variables:
- `UPLOAD_BUCKET`: Upload S3 bucket name
- `PROCESSING_BUCKET`: Processing S3 bucket name
- `JOBS_TABLE`: DynamoDB table name
- `NBP_API_URL`: NBP API endpoint
- `SES_FROM_EMAIL`: Email sender address
- `STAGE`: Deployment stage

## Outputs

After deployment, the stack exports:
- Upload bucket name
- Processing bucket name
- Jobs table name
- State machine ARN

## Cost Optimization

The infrastructure uses:
- DynamoDB on-demand billing
- Lambda with appropriate memory sizing
- S3 lifecycle policies for automatic deletion
- CloudWatch log retention (1 week)

## Security

- All S3 buckets have public access blocked
- S3 encryption enabled (SSE-S3)
- Lambda execution role follows least privilege
- DynamoDB point-in-time recovery (production only)

## Deployment Stages

### Development (`stage=dev`)
- Relaxed removal policies (DESTROY)
- Auto-delete S3 objects on stack deletion
- 1-week log retention

### Production (`stage=prod`)
- Retain removal policies
- Point-in-time recovery enabled
- Manual S3 cleanup required

## Notes

Before deploying:
1. Lambda functions must be built and published:
   ```bash
   cd ../../src
   dotnet publish -c Release
   ```
2. Ensure SES is configured and out of sandbox (or verify test emails)
3. Set appropriate AWS credentials and region

## Troubleshooting

**Error: "Lambda code not found"**
- Build and publish .NET Lambda projects first

**Error: "Bootstrap required"**
- Run `cdk bootstrap` for the target account/region

**Error: "SES email not verified"**
- Verify sender email in AWS SES console
