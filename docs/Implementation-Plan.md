# InvestTax Calculator - Step-by-Step Implementation Plan

**Version**: 1.0  
**Last Updated**: February 17, 2026  
**Target Phase**: MVP (Phase 1)  
**Estimated Duration**: 2-3 weeks  

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Technology Stack Summary](#technology-stack-summary)
4. [Implementation Phases](#implementation-phases)
5. [Detailed Step-by-Step Instructions](#detailed-step-by-step-instructions)
6. [Testing Strategy](#testing-strategy)
7. [Deployment Process](#deployment-process)
8. [Post-MVP Enhancements](#post-mvp-enhancements)

---

## Overview

This document provides a detailed, step-by-step implementation plan for building the InvestTax Calculator MVP. Each step is designed to be executed independently by a development agent, with clear inputs, outputs, and success criteria.

### Implementation Approach

- **Sequential Steps**: Each step builds on previous steps
- **Independent Execution**: Steps can be assigned to different agents
- **Clear Technology Specifications**: Exact versions and configurations provided
- **Testable Milestones**: Each step includes verification criteria
- **MVP Focus**: Excludes Phase 2 optimizations (caching, parallel processing, HTML reports)

---

## Prerequisites

### Required Software

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0.x | Lambda function development |
| AWS CLI | 2.15.x or later | AWS resource management |
| Node.js | 20.x LTS | AWS CDK infrastructure |
| Docker Desktop | 24.x or later | Local containerized environment |
| Git | 2.40+ | Version control |
| Visual Studio 2022 or VS Code | Latest | IDE |
| Postman or curl | Latest | API testing |

### AWS Account Setup

- AWS Account with Administrator access
- AWS CLI configured with credentials:
  ```bash
  aws configure
  # AWS Access Key ID: [your-key]
  # AWS Secret Access Key: [your-secret]
  # Default region: eu-central-1
  # Default output format: json
  ```
- SES configured and moved out of sandbox (or verified test email addresses)
- Budget alert set at $100/month

### Local Development Tools

- **LocalStack** (Community Edition 3.x): Mock AWS services locally
- **AWS SAM CLI** (1.116.x): Test Lambda functions locally
- **Docker Compose** (2.24.x): Container orchestration

---

## Technology Stack Summary

### Backend Services

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Lambda Runtime** | .NET 10 (Amazon Linux 2023) | 10.0.x | Serverless compute |
| **Lambda Functions** | C# | C# 13 | Business logic |
| **Lambda Deployment** | AWS Lambda .NET Annotations | 1.4.x | Simplified Lambda configuration |
| **DI Container** | Microsoft.Extensions.DependencyInjection | 10.0.x | Dependency injection |
| **CSV Parsing** | CsvHelper | 33.x | CSV file parsing |
| **HTTP Client** | System.Net.Http with Polly | 10.x / 8.3.x | NBP API calls with retry |
| **JSON Serialization** | System.Text.Json | 10.0.x | JSON handling |
| **Logging** | AWS.Lambda.Logging.AspNetCore | 4.x | CloudWatch logging |
| **AWS SDK** | AWSSDK.Core, AWSSDK.S3, AWSSDK.DynamoDBv2, AWSSDK.SimpleEmail | 3.7.x | AWS service integration |

### Orchestration

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Workflow Engine** | AWS Step Functions | Standard Workflow | Pipeline orchestration |
| **State Machine Definition** | Amazon States Language (ASL) | JSON format | Workflow definition |

### Storage

| Component | Technology | Configuration | Purpose |
|-----------|-----------|---------------|---------|
| **Object Storage** | Amazon S3 | Standard storage class, SSE-S3 | File storage |
| **Database** | Amazon DynamoDB | On-demand, single table | Job tracking |

### Infrastructure as Code

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **IaC Framework** | AWS CDK | 2.130.x | Infrastructure provisioning |
| **CDK Language** | TypeScript | 5.3.x | CDK stack definition |
| **CDK Constructs** | @aws-cdk/aws-lambda-dotnet | Latest | .NET Lambda constructs |

### CI/CD

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Source Control** | GitHub | N/A | Version control |
| **Build Pipeline** | GitHub Actions | N/A | Automated builds |
| **Package Manager** | NuGet | Latest | .NET package management |
| **Container Registry** | GitHub Container Registry (GHCR) | N/A | Docker images |

### Local Development

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **AWS Mock** | LocalStack | 3.2.x | Local AWS simulation |
| **Lambda Testing** | AWS SAM CLI | 1.116.x | Local Lambda invocation |
| **Container Runtime** | Docker Desktop | 24.x | Containerization |
| **Orchestration** | Docker Compose | 2.24.x | Multi-container setup |
| **Unit Testing** | xUnit | 2.9.x | Test framework |
| **Mocking** | Moq | 4.20.x | Test mocks |
| **Test Coverage** | Coverlet | 6.x | Code coverage |

### External APIs

| Service | Technology | Purpose |
|---------|-----------|---------|
| **Exchange Rates** | NBP API (api.nbp.pl) | REST API | PLN exchange rates |
| **Email Delivery** | Amazon SES | SMTP/API | Email sending |

---

## Implementation Phases

### Phase 1: Environment and Infrastructure Setup (Days 1-2)
- Step 1: Project structure and solution setup
- Step 2: Local development environment with Docker
- Step 3: AWS infrastructure with CDK
- Step 4: S3 buckets and DynamoDB tables

### Phase 2: Core Lambda Functions (Days 3-7)
- Step 5: Shared libraries and models
- Step 6: CSV Validator Lambda
- Step 7: Data Normalizer Lambda
- Step 8: NBP Rate Fetcher Lambda (no caching)
- Step 9: Tax Calculator Lambda (FIFO engine)
- Step 10: Report Generator Lambda (plain text)
- Step 11: Email Sender Lambda

### Phase 3: Orchestration and Integration (Days 8-10)
- Step 12: Step Functions state machine
- Step 13: S3 event trigger integration
- Step 14: End-to-end integration testing

### Phase 4: Testing and Quality Assurance (Days 11-12)
- Step 15: Unit test implementation (80% coverage target)
- Step 16: Integration test scenarios
- Step 17: Test data generation

### Phase 5: CI/CD and Documentation (Days 13-14)
- Step 18: GitHub Actions build pipeline
- Step 19: Deployment scripts and runbooks
- Step 20: User documentation and handoff

---

## Detailed Step-by-Step Instructions

---

## PHASE 1: ENVIRONMENT AND INFRASTRUCTURE SETUP

---

### Step 1: Project Structure and Solution Setup

**Duration**: 1-2 hours  
**Dependencies**: None  
**Prerequisites**: .NET 10 SDK installed

#### Objective
Create the foundational .NET solution structure with all necessary projects.

#### Technology Stack
- **.NET SDK**: 10.0.x
- **Solution Structure**: Multi-project solution
- **Project Type**: Class libraries and Lambda projects

#### Actions

1. **Create Solution Directory Structure**
   ```bash
   mkdir InvestTax.Calculator
   cd InvestTax.Calculator
   mkdir src tests infrastructure docs
   ```

2. **Create .NET Solution**
   ```bash
   dotnet new sln -n InvestTax.Calculator
   ```

3. **Create Projects**

   **Shared Core Library** (Domain models, interfaces):
   ```bash
   cd src
   dotnet new classlib -n InvestTax.Core -f net10.0
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Core/InvestTax.Core.csproj
   ```

   **Shared Infrastructure Library** (AWS SDK wrappers, utilities):
   ```bash
   dotnet new classlib -n InvestTax.Infrastructure -f net10.0
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   ```

   **Lambda Functions** (one project per function):
   ```bash
   dotnet new lambda.EmptyFunction -n InvestTax.Lambda.Validator -f net10.0
   dotnet new lambda.EmptyFunction -n InvestTax.Lambda.Normalizer -f net10.0
   dotnet new lambda.EmptyFunction -n InvestTax.Lambda.NBPClient -f net10.0
   dotnet new lambda.EmptyFunction -n InvestTax.Lambda.Calculator -f net10.0
   dotnet new lambda.EmptyFunction -n InvestTax.Lambda.ReportGenerator -f net10.0
   dotnet new lambda.EmptyFunction -n InvestTax.Lambda.EmailSender -f net10.0
   
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Lambda.Validator/InvestTax.Lambda.Validator.csproj
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Lambda.Normalizer/InvestTax.Lambda.Normalizer.csproj
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Lambda.NBPClient/InvestTax.Lambda.NBPClient.csproj
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Lambda.Calculator/InvestTax.Lambda.Calculator.csproj
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Lambda.ReportGenerator/InvestTax.Lambda.ReportGenerator.csproj
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Lambda.EmailSender/InvestTax.Lambda.EmailSender.csproj
   ```

   **Test Projects**:
   ```bash
   cd ../tests
   dotnet new xunit -n InvestTax.Core.Tests -f net10.0
   dotnet new xunit -n InvestTax.Lambda.Tests -f net10.0
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Core.Tests/InvestTax.Core.Tests.csproj
   dotnet sln ../InvestTax.Calculator.sln add InvestTax.Lambda.Tests/InvestTax.Lambda.Tests.csproj
   ```

4. **Add Project References**
   ```bash
   cd ../src
   dotnet add InvestTax.Infrastructure/InvestTax.Infrastructure.csproj reference InvestTax.Core/InvestTax.Core.csproj
   
   dotnet add InvestTax.Lambda.Validator/InvestTax.Lambda.Validator.csproj reference InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.Validator/InvestTax.Lambda.Validator.csproj reference InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   
   dotnet add InvestTax.Lambda.Normalizer/InvestTax.Lambda.Normalizer.csproj reference InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.Normalizer/InvestTax.Lambda.Normalizer.csproj reference InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   
   dotnet add InvestTax.Lambda.NBPClient/InvestTax.Lambda.NBPClient.csproj reference InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.NBPClient/InvestTax.Lambda.NBPClient.csproj reference InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   
   dotnet add InvestTax.Lambda.Calculator/InvestTax.Lambda.Calculator.csproj reference InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.Calculator/InvestTax.Lambda.Calculator.csproj reference InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   
   dotnet add InvestTax.Lambda.ReportGenerator/InvestTax.Lambda.ReportGenerator.csproj reference InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.ReportGenerator/InvestTax.Lambda.ReportGenerator.csproj reference InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   
   dotnet add InvestTax.Lambda.EmailSender/InvestTax.Lambda.EmailSender.csproj reference InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.EmailSender/InvestTax.Lambda.EmailSender.csproj reference InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   
   cd ../tests
   dotnet add InvestTax.Core.Tests/InvestTax.Core.Tests.csproj reference ../src/InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.Tests/InvestTax.Lambda.Tests.csproj reference ../src/InvestTax.Core/InvestTax.Core.csproj
   dotnet add InvestTax.Lambda.Tests/InvestTax.Lambda.Tests.csproj reference ../src/InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   ```

5. **Create Standard Directory Structure**
   ```bash
   cd ../src/InvestTax.Core
   mkdir Models Interfaces Enums Exceptions Validators
   
   cd ../InvestTax.Infrastructure
   mkdir AWS Services Utilities
   ```

6. **Create Configuration Files**

   **Directory.Build.props** (in src folder):
   ```xml
   <Project>
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <LangVersion>13.0</LangVersion>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
       <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
     </PropertyGroup>
   </Project>
   ```

   **.editorconfig** (root):
   ```ini
   root = true

   [*]
   charset = utf-8
   indent_style = space
   indent_size = 4
   insert_final_newline = true
   trim_trailing_whitespace = true

   [*.cs]
   # Naming conventions
   dotnet_naming_rule.interface_should_be_begins_with_i.severity = warning
   dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
   dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i
   ```

   **.gitignore**:
   ```
   ## .NET
   bin/
   obj/
   *.user
   *.suo
   
   ## AWS
   cdk.out/
   .aws-sam/
   
   ## IDEs
   .vs/
   .vscode/
   .idea/
   
   ## OS
   .DS_Store
   Thumbs.db
   
   ## Terraform
   *.tfstate
   *.tfstate.*
   .terraform/
   
   ## Logs
   *.log
   
   ## Test Results
   TestResults/
   coverage/
   ```

7. **Build Solution to Verify**
   ```bash
   cd ../..
   dotnet restore
   dotnet build
   ```

#### Expected Output
```
Solution Structure:
InvestTax.Calculator/
├── src/
│   ├── InvestTax.Core/
│   ├── InvestTax.Infrastructure/
│   ├── InvestTax.Lambda.Validator/
│   ├── InvestTax.Lambda.Normalizer/
│   ├── InvestTax.Lambda.NBPClient/
│   ├── InvestTax.Lambda.Calculator/
│   ├── InvestTax.Lambda.ReportGenerator/
│   └── InvestTax.Lambda.EmailSender/
├── tests/
│   ├── InvestTax.Core.Tests/
│   └── InvestTax.Lambda.Tests/
├── infrastructure/
├── docs/
├── InvestTax.Calculator.sln
├── .gitignore
└── .editorconfig
```

#### Success Criteria
- [ ] Solution builds without errors
- [ ] All projects reference correct dependencies
- [ ] Directory structure matches expected output
- [ ] `dotnet build` completes successfully

#### Troubleshooting
- **Error: SDK not found**: Install .NET 10 SDK from https://dot.net
- **Build errors**: Run `dotnet restore` first
- **Missing templates**: Run `dotnet new -i Amazon.Lambda.Templates`

---

### Step 2: Local Development Environment with Docker

**Duration**: 2-3 hours  
**Dependencies**: Step 1  
**Prerequisites**: Docker Desktop installed

#### Objective
Set up a containerized local development environment with LocalStack for AWS service mocking.

#### Technology Stack
- **LocalStack**: 3.2.x (Community Edition)
- **Docker Desktop**: 24.x
- **Docker Compose**: 2.24.x
- **AWS SAM CLI**: 1.116.x

#### Actions

1. **Create Docker Compose Configuration**

   Create `docker-compose.yml` in project root:
   ```yaml
   version: '3.8'
   
   services:
     localstack:
       image: localstack/localstack:3.2
       ports:
         - "4566:4566"            # LocalStack Gateway
         - "4510-4559:4510-4559"  # External services port range
       environment:
         - SERVICES=s3,dynamodb,stepfunctions,ses,lambda
         - DEBUG=1
         - DATA_DIR=/tmp/localstack/data
         - LAMBDA_EXECUTOR=docker
         - LAMBDA_DOCKER_NETWORK=investtax-network
         - DOCKER_HOST=unix:///var/run/docker.sock
       volumes:
         - "./localstack-data:/tmp/localstack"
         - "/var/run/docker.sock:/var/run/docker.sock"
       networks:
         - investtax-network
   
     dynamodb-admin:
       image: aaronshaf/dynamodb-admin:latest
       ports:
         - "8001:8001"
       environment:
         - DYNAMO_ENDPOINT=http://localstack:4566
         - AWS_REGION=eu-central-1
       depends_on:
         - localstack
       networks:
         - investtax-network
   
   networks:
     investtax-network:
       driver: bridge
   ```

2. **Create LocalStack Initialization Script**

   Create `infrastructure/localstack-init.sh`:
   ```bash
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
     --key-schema \
       AttributeName=JobId,KeyType=HASH \
     --billing-mode PAY_PER_REQUEST \
     --region $AWS_REGION
   
   # Verify SES email
   echo "Verifying SES email for testing..."
   aws --endpoint-url=$AWS_ENDPOINT ses verify-email-identity \
     --email-address test@example.com \
     --region $AWS_REGION
   
   echo "LocalStack initialization complete!"
   ```

   Make it executable:
   ```bash
   chmod +x infrastructure/localstack-init.sh
   ```

3. **Create AWS Configuration for Local Development**

   Create `src/InvestTax.Infrastructure/Configuration/LocalAwsConfig.cs`:
   ```csharp
   namespace InvestTax.Infrastructure.Configuration;
   
   public class LocalAwsConfig
   {
       public bool UseLocalStack { get; set; } = false;
       public string LocalStackEndpoint { get; set; } = "http://localhost:4566";
       public string Region { get; set; } = "eu-central-1";
       public string AccessKey { get; set; } = "test";
       public string SecretKey { get; set; } = "test";
   }
   ```

4. **Create appsettings.Development.json Template**

   Create template for each Lambda function (example for Validator):
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "System": "Information",
         "Microsoft": "Information"
       }
     },
     "AWS": {
       "UseLocalStack": true,
       "LocalStackEndpoint": "http://localhost:4566",
       "Region": "eu-central-1"
     },
     "S3": {
       "UploadBucket": "investtax-upload-local",
       "ProcessingBucket": "investtax-processing-local"
     },
     "DynamoDB": {
       "JobsTable": "InvestTax-Jobs-Local"
     }
   }
   ```

5. **Create SAM Local Configuration**

   Create `sam-local-template.yaml`:
   ```yaml
   AWSTemplateFormatVersion: '2010-09-09'
   Transform: AWS::Serverless-2016-10-31
   Description: Local development template for InvestTax Calculator
   
   Globals:
     Function:
       Timeout: 300
       MemorySize: 512
       Runtime: dotnet10
       Environment:
         Variables:
           AWS_SAM_LOCAL: true
           LOCALSTACK_HOSTNAME: localstack
   
   Resources:
     ValidatorFunction:
       Type: AWS::Serverless::Function
       Properties:
         Handler: InvestTax.Lambda.Validator::InvestTax.Lambda.Validator.Function::FunctionHandler
         CodeUri: ./src/InvestTax.Lambda.Validator/bin/Debug/net10.0/
         Environment:
           Variables:
             UPLOAD_BUCKET: investtax-upload-local
             PROCESSING_BUCKET: investtax-processing-local
   ```

6. **Create Helper Scripts**

   Create `scripts/start-local.sh`:
   ```bash
   #!/bin/bash
   
   echo "Starting LocalStack environment..."
   docker-compose up -d
   
   echo "Waiting for LocalStack to be ready..."
   sleep 10
   
   echo "Initializing AWS resources..."
   ./infrastructure/localstack-init.sh
   
   echo "Local environment ready!"
   echo "LocalStack Gateway: http://localhost:4566"
   echo "DynamoDB Admin: http://localhost:8001"
   ```

   Create `scripts/stop-local.sh`:
   ```bash
   #!/bin/bash
   
   echo "Stopping LocalStack environment..."
   docker-compose down
   
   echo "Local environment stopped."
   ```

   Make scripts executable:
   ```bash
   chmod +x scripts/start-local.sh scripts/stop-local.sh
   ```

7. **Test Local Environment**
   ```bash
   # Start LocalStack
   ./scripts/start-local.sh
   
   # Verify S3 buckets
   aws --endpoint-url=http://localhost:4566 s3 ls
   
   # Verify DynamoDB tables
   aws --endpoint-url=http://localhost:4566 dynamodb list-tables
   
   # Open DynamoDB Admin in browser
   # http://localhost:8001
   ```

#### Expected Output
```
LocalStack Services:
- S3: http://localhost:4566 (buckets: investtax-upload-local, investtax-processing-local)
- DynamoDB: http://localhost:4566 (table: InvestTax-Jobs-Local)
- SES: http://localhost:4566 (verified: test@example.com)
- DynamoDB Admin UI: http://localhost:8001
```

#### Success Criteria
- [ ] Docker containers running successfully
- [ ] LocalStack accessible at http://localhost:4566
- [ ] S3 buckets created
- [ ] DynamoDB table created
- [ ] DynamoDB Admin UI accessible
- [ ] Can list resources via AWS CLI

#### Troubleshooting
- **Docker not starting**: Ensure Docker Desktop is running
- **Port conflicts**: Change ports in docker-compose.yml
- **Permission errors**: Run scripts with appropriate permissions
- **LocalStack not ready**: Increase sleep time in init script

---

### Step 3: AWS Infrastructure with CDK (TypeScript)

**Duration**: 2-3 hours  
**Dependencies**: Step 1  
**Prerequisites**: Node.js 20.x, AWS CLI configured

#### Objective
Create AWS CDK infrastructure code to provision all AWS resources.

#### Technology Stack
- **AWS CDK**: 2.130.x
- **CDK Language**: TypeScript 5.3.x
- **Node.js**: 20.x LTS

#### Actions

1. **Initialize CDK Project**
   ```bash
   cd infrastructure
   mkdir cdk
   cd cdk
   npx aws-cdk@2.130.0 init app --language typescript
   ```

2. **Install Required CDK Dependencies**
   ```bash
   npm install @aws-cdk/aws-lambda-dotnet \
     @aws-cdk/aws-stepfunctions-tasks \
     aws-cdk-lib@^2.130.0 \
     constructs@^10.0.0
   ```

3. **Create CDK Stack for MVP**

   Edit `lib/investtax-stack.ts`:
   ```typescript
   import * as cdk from 'aws-cdk-lib';
   import * as s3 from 'aws-cdk-lib/aws-s3';
   import * as dynamodb from 'aws-cdk-lib/aws-dynamodb';
   import * as lambda from 'aws-cdk-lib/aws-lambda';
   import * as iam from 'aws-cdk-lib/aws-iam';
   import * as ses from 'aws-cdk-lib/aws-ses';
   import * as logs from 'aws-cdk-lib/aws-logs';
   import * as sfn from 'aws-cdk-lib/aws-stepfunctions';
   import * as tasks from 'aws-cdk-lib/aws-stepfunctions-tasks';
   import * as s3n from 'aws-cdk-lib/aws-s3-notifications';
   import { Construct } from 'constructs';
   
   export interface InvestTaxStackProps extends cdk.StackProps {
     stage: string;
   }
   
   export class InvestTaxStack extends cdk.Stack {
     constructor(scope: Construct, id: string, props: InvestTaxStackProps) {
       super(scope, id, props);
   
       const stage = props.stage;
   
       // ==================== S3 BUCKETS ====================
       
       // Upload bucket (user uploads)
       const uploadBucket = new s3.Bucket(this, 'UploadBucket', {
         bucketName: `investtax-upload-${stage}`,
         encryption: s3.BucketEncryption.S3_MANAGED,
         blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
         lifecycleRules: [
           {
             id: 'DeleteAfter7Days',
             expiration: cdk.Duration.days(7),
           },
         ],
         removalPolicy: stage === 'prod' ? cdk.RemovalPolicy.RETAIN : cdk.RemovalPolicy.DESTROY,
         autoDeleteObjects: stage !== 'prod',
       });
   
       // Processing bucket (intermediate files)
       const processingBucket = new s3.Bucket(this, 'ProcessingBucket', {
         bucketName: `investtax-processing-${stage}`,
         encryption: s3.BucketEncryption.S3_MANAGED,
         blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
         lifecycleRules: [
           {
             id: 'DeleteAfter1Day',
             expiration: cdk.Duration.days(1),
           },
         ],
         removalPolicy: stage === 'prod' ? cdk.RemovalPolicy.RETAIN : cdk.RemovalPolicy.DESTROY,
         autoDeleteObjects: stage !== 'prod',
       });
   
       // ==================== DYNAMODB TABLE ====================
       
       const jobsTable = new dynamodb.Table(this, 'JobsTable', {
         tableName: `InvestTax-Jobs-${stage}`,
         partitionKey: {
           name: 'JobId',
           type: dynamodb.AttributeType.STRING,
         },
         billingMode: dynamodb.BillingMode.PAY_PER_REQUEST,
         encryption: dynamodb.TableEncryption.AWS_MANAGED,
         removalPolicy: stage === 'prod' ? cdk.RemovalPolicy.RETAIN : cdk.RemovalPolicy.DESTROY,
         pointInTimeRecovery: stage === 'prod',
       });
   
       // GSI for querying by status
       jobsTable.addGlobalSecondaryIndex({
         indexName: 'StatusIndex',
         partitionKey: {
           name: 'Status',
           type: dynamodb.AttributeType.STRING,
         },
         sortKey: {
           name: 'CreatedAt',
           type: dynamodb.AttributeType.STRING,
         },
       });
   
       // ==================== IAM ROLES ====================
       
       // Common Lambda execution role
       const lambdaExecutionRole = new iam.Role(this, 'LambdaExecutionRole', {
         assumedBy: new iam.ServicePrincipal('lambda.amazonaws.com'),
         managedPolicies: [
           iam.ManagedPolicy.fromAwsManagedPolicyName('service-role/AWSLambdaBasicExecutionRole'),
         ],
       });
   
       // Grant permissions
       uploadBucket.grantReadWrite(lambdaExecutionRole);
       processingBucket.grantReadWrite(lambdaExecutionRole);
       jobsTable.grantReadWriteData(lambdaExecutionRole);
   
       // SES permissions
       lambdaExecutionRole.addToPolicy(new iam.PolicyStatement({
         actions: ['ses:SendEmail', 'ses:SendRawEmail'],
         resources: ['*'],
       }));
   
       // ==================== LAMBDA FUNCTIONS ====================
       
       // Validator Lambda
       const validatorFunction = new lambda.Function(this, 'ValidatorFunction', {
         functionName: `InvestTax-Validator-${stage}`,
         runtime: lambda.Runtime.DOTNET_10,
         handler: 'InvestTax.Lambda.Validator::InvestTax.Lambda.Validator.Function::FunctionHandler',
         code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.Validator/bin/Release/net10.0/publish'),
         memorySize: 512,
         timeout: cdk.Duration.minutes(5),
         role: lambdaExecutionRole,
         environment: {
           UPLOAD_BUCKET: uploadBucket.bucketName,
           PROCESSING_BUCKET: processingBucket.bucketName,
           JOBS_TABLE: jobsTable.tableName,
           STAGE: stage,
         },
         logRetention: logs.RetentionDays.ONE_WEEK,
       });
   
       // Normalizer Lambda
       const normalizerFunction = new lambda.Function(this, 'NormalizerFunction', {
         functionName: `InvestTax-Normalizer-${stage}`,
         runtime: lambda.Runtime.DOTNET_10,
         handler: 'InvestTax.Lambda.Normalizer::InvestTax.Lambda.Normalizer.Function::FunctionHandler',
         code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.Normalizer/bin/Release/net10.0/publish'),
         memorySize: 1024,
         timeout: cdk.Duration.minutes(10),
         role: lambdaExecutionRole,
         environment: {
           PROCESSING_BUCKET: processingBucket.bucketName,
           JOBS_TABLE: jobsTable.tableName,
           STAGE: stage,
         },
         logRetention: logs.RetentionDays.ONE_WEEK,
       });
   
       // NBP Client Lambda
       const nbpClientFunction = new lambda.Function(this, 'NBPClientFunction', {
         functionName: `InvestTax-NBPClient-${stage}`,
         runtime: lambda.Runtime.DOTNET_10,
         handler: 'InvestTax.Lambda.NBPClient::InvestTax.Lambda.NBPClient.Function::FunctionHandler',
         code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.NBPClient/bin/Release/net10.0/publish'),
         memorySize: 256,
         timeout: cdk.Duration.minutes(5),
         role: lambdaExecutionRole,
         environment: {
           NBP_API_URL: 'https://api.nbp.pl/api',
           STAGE: stage,
         },
         logRetention: logs.RetentionDays.ONE_WEEK,
       });
   
       // Calculator Lambda
       const calculatorFunction = new lambda.Function(this, 'CalculatorFunction', {
         functionName: `InvestTax-Calculator-${stage}`,
         runtime: lambda.Runtime.DOTNET_10,
         handler: 'InvestTax.Lambda.Calculator::InvestTax.Lambda.Calculator.Function::FunctionHandler',
         code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.Calculator/bin/Release/net10.0/publish'),
         memorySize: 2048,
         timeout: cdk.Duration.minutes(10),
         role: lambdaExecutionRole,
         environment: {
           PROCESSING_BUCKET: processingBucket.bucketName,
           JOBS_TABLE: jobsTable.tableName,
           STAGE: stage,
         },
         logRetention: logs.RetentionDays.ONE_WEEK,
       });
   
       // Report Generator Lambda
       const reportGeneratorFunction = new lambda.Function(this, 'ReportGeneratorFunction', {
         functionName: `InvestTax-ReportGenerator-${stage}`,
         runtime: lambda.Runtime.DOTNET_10,
         handler: 'InvestTax.Lambda.ReportGenerator::InvestTax.Lambda.ReportGenerator.Function::FunctionHandler',
         code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.ReportGenerator/bin/Release/net10.0/publish'),
         memorySize: 512,
         timeout: cdk.Duration.minutes(3),
         role: lambdaExecutionRole,
         environment: {
           PROCESSING_BUCKET: processingBucket.bucketName,
           JOBS_TABLE: jobsTable.tableName,
           STAGE: stage,
         },
         logRetention: logs.RetentionDays.ONE_WEEK,
       });
   
       // Email Sender Lambda
       const emailSenderFunction = new lambda.Function(this, 'EmailSenderFunction', {
         functionName: `InvestTax-EmailSender-${stage}`,
         runtime: lambda.Runtime.DOTNET_10,
         handler: 'InvestTax.Lambda.EmailSender::InvestTax.Lambda.EmailSender.Function::FunctionHandler',
         code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.EmailSender/bin/Release/net10.0/publish'),
         memorySize: 256,
         timeout: cdk.Duration.minutes(2),
         role: lambdaExecutionRole,
         environment: {
           SES_FROM_EMAIL: `no-reply@investtax-${stage}.example.com`,
           PROCESSING_BUCKET: processingBucket.bucketName,
           JOBS_TABLE: jobsTable.tableName,
           STAGE: stage,
         },
         logRetention: logs.RetentionDays.ONE_WEEK,
       });
   
       // ==================== STEP FUNCTIONS STATE MACHINE ====================
       
       // Define Step Functions tasks
       const validateTask = new tasks.LambdaInvoke(this, 'ValidateCSV', {
         lambdaFunction: validatorFunction,
         outputPath: '$.Payload',
       });
   
       const normalizeTask = new tasks.LambdaInvoke(this, 'NormalizeData', {
         lambdaFunction: normalizerFunction,
         outputPath: '$.Payload',
       });
   
       const fetchRatesTask = new tasks.LambdaInvoke(this, 'FetchNBPRates', {
         lambdaFunction: nbpClientFunction,
         outputPath: '$.Payload',
       });
   
       const calculateTask = new tasks.LambdaInvoke(this, 'CalculateTax', {
         lambdaFunction: calculatorFunction,
         outputPath: '$.Payload',
       });
   
       const generateReportTask = new tasks.LambdaInvoke(this, 'GenerateReport', {
         lambdaFunction: reportGeneratorFunction,
         outputPath: '$.Payload',
       });
   
       const sendEmailTask = new tasks.LambdaInvoke(this, 'SendEmail', {
         lambdaFunction: emailSenderFunction,
         outputPath: '$.Payload',
       });
   
       // Define workflow
       const definition = validateTask
         .next(normalizeTask)
         .next(fetchRatesTask)
         .next(calculateTask)
         .next(generateReportTask)
         .next(sendEmailTask);
   
       // Create state machine
       const stateMachine = new sfn.StateMachine(this, 'InvestTaxWorkflow', {
         stateMachineName: `InvestTax-Workflow-${stage}`,
         definition: definition,
         timeout: cdk.Duration.minutes(15),
         logs: {
           destination: new logs.LogGroup(this, 'StateMachineLogs', {
             logGroupName: `/aws/stepfunctions/InvestTax-Workflow-${stage}`,
             retention: logs.RetentionDays.ONE_WEEK,
           }),
           level: sfn.LogLevel.ALL,
         },
       });
   
       // Grant Step Functions permission to invoke Lambdas
       validatorFunction.grantInvoke(stateMachine);
       normalizerFunction.grantInvoke(stateMachine);
       nbpClientFunction.grantInvoke(stateMachine);
       calculatorFunction.grantInvoke(stateMachine);
       reportGeneratorFunction.grantInvoke(stateMachine);
       emailSenderFunction.grantInvoke(stateMachine);
   
       // ==================== S3 EVENT TRIGGER ====================
       
       // Lambda to trigger Step Functions
       const triggerFunction = new lambda.Function(this, 'TriggerFunction', {
         functionName: `InvestTax-Trigger-${stage}`,
         runtime: lambda.Runtime.DOTNET_10,
         handler: 'index.handler',
         code: lambda.Code.fromInline(`
           // Inline handler to start Step Functions execution
           exports.handler = async (event) => {
             // Implementation in Step 12
           };
         `),
         environment: {
           STATE_MACHINE_ARN: stateMachine.stateMachineArn,
         },
       });
   
       stateMachine.grantStartExecution(triggerFunction);
       uploadBucket.addEventNotification(
         s3.EventType.OBJECT_CREATED,
         new s3n.LambdaDestination(triggerFunction)
       );
   
       // ==================== OUTPUTS ====================
       
       new cdk.CfnOutput(this, 'UploadBucketName', {
         value: uploadBucket.bucketName,
         description: 'S3 Upload Bucket Name',
       });
   
       new cdk.CfnOutput(this, 'ProcessingBucketName', {
         value: processingBucket.bucketName,
         description: 'S3 Processing Bucket Name',
       });
   
       new cdk.CfnOutput(this, 'JobsTableName', {
         value: jobsTable.tableName,
         description: 'DynamoDB Jobs Table Name',
       });
   
       new cdk.CfnOutput(this, 'StateMachineArn', {
         value: stateMachine.stateMachineArn,
         description: 'Step Functions State Machine ARN',
       });
     }
   }
   ```

4. **Update CDK App Entry Point**

   Edit `bin/cdk.ts`:
   ```typescript
   #!/usr/bin/env node
   import 'source-map-support/register';
   import * as cdk from 'aws-cdk-lib';
   import { InvestTaxStack } from '../lib/investtax-stack';
   
   const app = new cdk.App();
   
   const stage = app.node.tryGetContext('stage') || 'dev';
   
   new InvestTaxStack(app, `InvestTaxStack-${stage}`, {
     stage: stage,
     env: {
       account: process.env.CDK_DEFAULT_ACCOUNT,
       region: 'eu-central-1',
     },
     description: `InvestTax Calculator - ${stage.toUpperCase()} environment`,
   });
   ```

5. **Create CDK Configuration**

   Edit `cdk.json`:
   ```json
   {
     "app": "npx ts-node --prefer-ts-exts bin/cdk.ts",
     "context": {
       "@aws-cdk/aws-lambda:recognizeLayerVersion": true,
       "@aws-cdk/core:checkSecretUsage": true,
       "@aws-cdk/core:target-partitions": ["aws", "aws-cn"],
       "@aws-cdk-containers/ecs-service-extensions:enableDefaultLogDriver": true,
       "@aws-cdk/aws-ec2:uniqueImdsv2TemplateName": true,
       "@aws-cdk/aws-ecs:arnFormatIncludesClusterName": true,
       "@aws-cdk/aws-iam:minimizePolicies": true,
       "@aws-cdk/core:validateSnapshotRemovalPolicy": true,
       "@aws-cdk/aws-codepipeline:crossAccountKeyAliasStackSafeResourceName": true,
       "@aws-cdk/aws-s3:createDefaultLoggingPolicy": true,
       "@aws-cdk/aws-sns-subscriptions:restrictSqsDescryption": true,
       "@aws-cdk/aws-apigateway:disableCloudWatchRole": true,
       "@aws-cdk/core:enablePartitionLiterals": true,
       "@aws-cdk/aws-events:eventsTargetQueueSameAccount": true,
       "@aws-cdk/aws-iam:standardizedServicePrincipals": true,
       "@aws-cdk/aws-ecs:disableExplicitDeploymentControllerForCircuitBreaker": true,
       "@aws-cdk/aws-iam:importedRoleStackSafeDefaultPolicyName": true,
       "@aws-cdk/aws-s3:serverAccessLogsUseBucketPolicy": true,
       "@aws-cdk/aws-route53-patters:useCertificate": true,
       "@aws-cdk/customresources:installLatestAwsSdkDefault": false,
       "@aws-cdk/aws-rds:databaseProxyUniqueResourceName": true,
       "@aws-cdk/aws-codedeploy:removeAlarmsFromDeploymentGroup": true,
       "@aws-cdk/aws-apigateway:authorizerChangeDeploymentLogicalId": true,
       "@aws-cdk/aws-ec2:launchTemplateDefaultUserData": true,
       "@aws-cdk/aws-secretsmanager:useAttachedSecretResourcePolicyForSecretTargetAttachments": true,
       "@aws-cdk/aws-redshift:columnId": true,
       "@aws-cdk/aws-stepfunctions-tasks:enableEmrServicePolicyV2": true,
       "@aws-cdk/aws-ec2:restrictDefaultSecurityGroup": true,
       "@aws-cdk/aws-apigateway:requestValidatorUniqueId": true,
       "@aws-cdk/aws-kms:aliasNameRef": true,
       "@aws-cdk/aws-autoscaling:generateLaunchTemplateInsteadOfLaunchConfig": true,
       "@aws-cdk/core:includePrefixInUniqueNameGeneration": true,
       "@aws-cdk/aws-efs:denyAnonymousAccess": true,
       "@aws-cdk/aws-opensearchservice:enableOpensearchMultiAzWithStandby": true,
       "@aws-cdk/aws-lambda-nodejs:useLatestRuntimeVersion": true,
       "@aws-cdk/aws-efs:mountTargetOrderInsensitiveLogicalId": true,
       "@aws-cdk/aws-rds:auroraClusterChangeScopeOfInstanceParameterGroupWithEachParameters": true,
       "@aws-cdk/aws-appsync:useArnForSourceApiAssociationIdentifier": true,
       "@aws-cdk/aws-rds:preventRenderingDeprecatedCredentials": true,
       "@aws-cdk/aws-codepipeline-actions:useNewDefaultBranchForCodeCommitSource": true,
       "@aws-cdk/aws-cloudwatch-actions:changeLambdaPermissionLogicalIdForLambdaAction": true,
       "@aws-cdk/aws-codepipeline:crossAccountKeysDefaultValueToFalse": true,
       "@aws-cdk/aws-codepipeline:defaultPipelineTypeToV2": true,
       "@aws-cdk/aws-kms:reduceCrossAccountRegionPolicyScope": true,
       "@aws-cdk/aws-eks:nodegroupNameAttribute": true,
       "@aws-cdk/aws-ec2:ebsDefaultGp3Volume": true,
       "@aws-cdk/aws-ecs:removeDefaultDeploymentAlarm": true
     }
   }
   ```

6. **Bootstrap CDK (First-time only)**
   ```bash
   npm run build
   npx cdk bootstrap aws://ACCOUNT-ID/eu-central-1
   ```

7. **Deploy to Dev (After Lambda functions built)**
   ```bash
   # Will be used in later steps after building Lambdas
   npx cdk synth -c stage=dev
   npx cdk deploy -c stage=dev
   ```

#### Expected Output
```
CDK Project Structure:
infrastructure/cdk/
├── bin/
│   └── cdk.ts
├── lib/
│   └── investtax-stack.ts
├── cdk.json
├── package.json
└── tsconfig.json

CDK synthesizes CloudFormation template for:
- 2 S3 buckets
- 1 DynamoDB table
- 6 Lambda functions
- 1 Step Functions state machine
- IAM roles and policies
```

#### Success Criteria
- [ ] CDK project builds without errors (`npm run build`)
- [ ] CDK synthesizes successfully (`cdk synth`)
- [ ] TypeScript compiles without errors
- [ ] CloudFormation template generated in `cdk.out/`

#### Troubleshooting
- **CDK not found**: Run `npm install -g aws-cdk`
- **TypeScript errors**: Run `npm install` to install dependencies
- **Bootstrap errors**: Ensure AWS CLI is configured correctly

---

### Step 4: Core Domain Models and Interfaces

**Duration**: 2-3 hours  
**Dependencies**: Step 1  
**Prerequisites**: Solution created

#### Objective
Define all core domain models, DTOs, enums, and interfaces used across Lambda functions.

#### Technology Stack
- **Language**: C# 13
- **Framework**: .NET 10
- **Validation**: System.ComponentModel.DataAnnotations

#### Actions

1. **Create Enums**

   Create `src/InvestTax.Core/Enums/JobStatus.cs`:
   ```csharp
   namespace InvestTax.Core.Enums;
   
   /// <summary>
   /// Represents the processing status of a tax calculation job
   /// </summary>
   public enum JobStatus
   {
       /// <summary>Job created, awaiting validation</summary>
       Created,
       
       /// <summary>CSV validation in progress</summary>
       Validating,
       
       /// <summary>Data normalization in progress</summary>
       Normalizing,
       
       /// <summary>Fetching NBP exchange rates</summary>
       FetchingRates,
       
       /// <summary>Tax calculation in progress</summary>
       Calculating,
       
       /// <summary>Generating report</summary>
       GeneratingReport,
       
       /// <summary>Sending email</summary>
       SendingEmail,
       
       /// <summary>Job completed successfully</summary>
       Completed,
       
       /// <summary>Job failed with error</summary>
       Failed
   }
   ```

   Create `src/InvestTax.Core/Enums/TransactionAction.cs`:
   ```csharp
   namespace InvestTax.Core.Enums;
   
   /// <summary>
   /// Type of transaction action
   /// </summary>
   public enum TransactionAction
   {
       /// <summary>Buy/purchase transaction</summary>
       Buy,
       
       /// <summary>Sell transaction</summary>
       Sell
   }
   ```

   Create `src/InvestTax.Core/Enums/Currency.cs`:
   ```csharp
   namespace InvestTax.Core.Enums;
   
   /// <summary>
   /// Supported currencies
   /// </summary>
   public enum Currency
   {
       PLN,
       USD,
       EUR,
       GBP,
       CHF,
       JPY,
       CAD,
       AUD
   }
   ```

2. **Create Domain Models**

   Create `src/InvestTax.Core/Models/Transaction.cs`:
   ```csharp
   using InvestTax.Core.Enums;
   using System.ComponentModel.DataAnnotations;
   
   namespace InvestTax.Core.Models;
   
   /// <summary>
   /// Represents a single buy or sell transaction
   /// </summary>
   public class Transaction
   {
       /// <summary>Buy or Sell action</summary>
       [Required]
       public TransactionAction Action { get; set; }
       
       /// <summary>Transaction timestamp (UTC)</summary>
       [Required]
       public DateTime Time { get; set; }
       
       /// <summary>ISIN code of the security</summary>
       [Required]
       [StringLength(12, MinimumLength = 12)]
       public string ISIN { get; set; } = string.Empty;
       
       /// <summary>Ticker symbol</summary>
       public string? Ticker { get; set; }
       
       /// <summary>Security name or notes</summary>
       public string? Name { get; set; }
       
       /// <summary>Transaction ID from broker</summary>
       [Required]
       public string TransactionId { get; set; } = string.Empty;
       
       /// <summary>Number of shares</summary>
       [Required]
       [Range(0.000001, double.MaxValue)]
       public decimal Shares { get; set; }
       
       /// <summary>Price per share in original currency</summary>
       [Required]
       [Range(0, double.MaxValue)]
       public decimal PricePerShare { get; set; }
       
       /// <summary>Currency of price per share</summary>
       [Required]
       public Currency PriceCurrency { get; set; }
       
       /// <summary>Exchange rate used by broker (if provided)</summary>
       public decimal? BrokerExchangeRate { get; set; }
       
       /// <summary>Transaction result/total in result currency</summary>
       [Required]
       public decimal Result { get; set; }
       
       /// <summary>Currency of result</summary>
       [Required]
       public Currency ResultCurrency { get; set; }
       
       /// <summary>Final total amount in total currency</summary>
       [Required]
       public decimal Total { get; set; }
       
       /// <summary>Currency of total</summary>
       [Required]
       public Currency TotalCurrency { get; set; }
       
       /// <summary>NBP exchange rate for trade date (PLN per unit of foreign currency)</summary>
       public decimal? NBPExchangeRate { get; set; }
       
       /// <summary>Total value in PLN (calculated)</summary>
       public decimal? TotalPLN { get; set; }
       
       /// <summary>Transaction costs/commissions in PLN</summary>
       public decimal TransactionCostsPLN { get; set; }
   }
   ```

   Create `src/InvestTax.Core/Models/TaxCalculation.cs`:
   ```csharp
   namespace InvestTax.Core.Models;
   
   /// <summary>
   /// Result of FIFO tax calculation for a single sell matched to buy(s)
   /// </summary>
   public class TaxCalculation
   {
       /// <summary>ISIN of the security</summary>
       public string ISIN { get; set; } = string.Empty;
       
       /// <summary>Sell transaction</summary>
       public Transaction SellTransaction { get; set; } = null!;
       
       /// <summary>Matched buy transactions (FIFO order)</summary>
       public List<MatchedBuy> MatchedBuys { get; set; } = new();
       
       /// <summary>Total cost basis in PLN</summary>
       public decimal CostBasisPLN { get; set; }
       
       /// <summary>Total proceeds in PLN</summary>
       public decimal ProceedsPLN { get; set; }
       
       /// <summary>Capital gain/loss in PLN</summary>
       public decimal GainLossPLN { get; set; }
       
       /// <summary>Whether this is a gain or loss</summary>
       public bool IsGain => GainLossPLN > 0;
   }
   
   /// <summary>
   /// Represents a buy transaction matched to a sell via FIFO
   /// </summary>
   public class MatchedBuy
   {
       /// <summary>Buy transaction</summary>
       public Transaction BuyTransaction { get; set; } = null!;
       
       /// <summary>Number of shares matched from this buy</summary>
       public decimal SharesMatched { get; set; }
       
       /// <summary>Cost basis for matched shares in PLN</summary>
       public decimal CostBasisPLN { get; set; }
   }
   ```

   Create `src/InvestTax.Core/Models/TaxSummary.cs`:
   ```csharp
   namespace InvestTax.Core.Models;
   
   /// <summary>
   /// Summary of all tax calculations for the year
   /// </summary>
   public class TaxSummary
   {
       /// <summary>Tax year</summary>
       public int Year { get; set; }
       
       /// <summary>All individual calculations</summary>
       public List<TaxCalculation> Calculations { get; set; } = new();
       
       /// <summary>Total capital gains in PLN</summary>
       public decimal TotalGainsPLN { get; set; }
       
       /// <summary>Total capital losses in PLN</summary>
       public decimal TotalLossesPLN { get; set; }
       
       /// <summary>Net taxable amount in PLN</summary>
       public decimal NetTaxableAmountPLN { get; set; }
       
       /// <summary>Tax owed at 19% (informational)</summary>
       public decimal EstimatedTaxPLN => NetTaxableAmountPLN * 0.19m;
       
       /// <summary>Number of taxable transactions</summary>
       public int TotalTransactions { get; set; }
       
       /// <summary>Warning messages</summary>
       public List<string> Warnings { get; set; } = new();
   }
   ```

   Create `src/InvestTax.Core/Models/Job.cs`:
   ```csharp
   using InvestTax.Core.Enums;
   
   namespace InvestTax.Core.Models;
   
   /// <summary>
   /// Job tracking entity stored in DynamoDB
   /// </summary>
   public class Job
   {
       /// <summary>Unique job identifier (GUID)</summary>
       public string JobId { get; set; } = Guid.NewGuid().ToString();
       
       /// <summary>User email address</summary>
       public string Email { get; set; } = string.Empty;
       
       /// <summary>S3 key of uploaded file</summary>
       public string S3Key { get; set; } = string.Empty;
       
       /// <summary>Current job status</summary>
       public JobStatus Status { get; set; } = JobStatus.Created;
       
       /// <summary>Job creation timestamp (ISO 8601)</summary>
       public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
       
       /// <summary>Last update timestamp (ISO 8601)</summary>
       public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
       
       /// <summary>Job completion timestamp (ISO 8601)</summary>
       public string? CompletedAt { get; set; }
       
       /// <summary>Error message if status is Failed</summary>
       public string? ErrorMessage { get; set; }
       
       /// <summary>Step Functions execution ARN</summary>
       public string? ExecutionArn { get; set; }
       
       /// <summary>Number of transactions processed</summary>
       public int? TransactionCount { get; set; }
       
       /// <summary>Processing duration in seconds</summary>
       public double? DurationSeconds { get; set; }
   }
   ```

3. **Create DTOs (Data Transfer Objects)**

   Create `src/InvestTax.Core/Models/NBPExchangeRateResponse.cs`:
   ```csharp
   namespace InvestTax.Core.Models;
   
   /// <summary>
   /// Response from NBP API for exchange rates
   /// </summary>
   public class NBPExchangeRateResponse
   {
       public string Table { get; set; } = string.Empty;
       public string Currency { get; set; } = string.Empty;
       public string Code { get; set; } = string.Empty;
       public List<NBPRate> Rates { get; set; } = new();
   }
   
   public class NBPRate
   {
       public string No { get; set; } = string.Empty;
       public string EffectiveDate { get; set; } = string.Empty;
       public decimal Mid { get; set; }
   }
   ```

   Create `src/InvestTax.Core/Models/LambdaInput.cs`:
   ```csharp
   namespace InvestTax.Core.Models;
   
   /// <summary>
   /// Common input structure for Step Functions Lambda invocations
   /// </summary>
   public class LambdaInput
   {
       public string JobId { get; set; } = string.Empty;
       public string Email { get; set; } = string.Empty;
       public string S3Key { get; set; } = string.Empty;
       public string UploadBucket { get; set; } = string.Empty;
       public string ProcessingBucket { get; set; } = string.Empty;
   }
   
   /// <summary>
   /// Common output structure for Step Functions Lambda invocations
   /// </summary>
   public class LambdaOutput
   {
       public bool Success { get; set; }
       public string JobId { get; set; } = string.Empty;
       public string? ErrorMessage { get; set; }
       public Dictionary<string, string> Metadata { get; set; } = new();
   }
   ```

4. **Create Service Interfaces**

   Create `src/InvestTax.Core/Interfaces/IS3Service.cs`:
   ```csharp
   namespace InvestTax.Core.Interfaces;
   
   /// <summary>
   /// Abstraction for S3 operations
   /// </summary>
   public interface IS3Service
   {
       Task<string> GetObjectAsStringAsync(string bucketName, string key, CancellationToken cancellationToken = default);
       Task<Stream> GetObjectStreamAsync(string bucketName, string key, CancellationToken cancellationToken = default);
       Task PutObjectAsync(string bucketName, string key, string content, CancellationToken cancellationToken = default);
       Task PutObjectAsync(string bucketName, string key, Stream stream, CancellationToken cancellationToken = default);
       Task<bool> ObjectExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default);
       Task DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default);
       Task<Dictionary<string, string>> GetObjectMetadataAsync(string bucketName, string key, CancellationToken cancellationToken = default);
   }
   ```

   Create `src/InvestTax.Core/Interfaces/IDynamoDbService.cs`:
   ```csharp
   using InvestTax.Core.Models;
   
   namespace InvestTax.Core.Interfaces;
   
   /// <summary>
   /// Abstraction for DynamoDB operations
   /// </summary>
   public interface IDynamoDbService
   {
       Task<Job?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
       Task SaveJobAsync(Job job, CancellationToken cancellationToken = default);
       Task UpdateJobStatusAsync(string jobId, JobStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
       Task<List<Job>> GetJobsByStatusAsync(JobStatus status, int limit = 100, CancellationToken cancellationToken = default);
   }
   ```

   Create `src/InvestTax.Core/Interfaces/INBPApiClient.cs`:
   ```csharp
   using InvestTax.Core.Enums;
   
   namespace InvestTax.Core.Interfaces;
   
   /// <summary>
   /// Client for NBP exchange rate API
   /// </summary>
   public interface INBPApiClient
   {
       /// <summary>
       /// Get exchange rate for a specific date and currency
       /// </summary>
       /// <param name="currency">Currency code (USD, EUR, etc.)</param>
       /// <param name="date">Date for exchange rate</param>
       /// <param name="cancellationToken">Cancellation token</param>
       /// <returns>Exchange rate (PLN per unit of foreign currency)</returns>
       Task<decimal> GetExchangeRateAsync(Currency currency, DateOnly date, CancellationToken cancellationToken = default);
       
       /// <summary>
       /// Get exchange rates for multiple dates (batch)
       /// </summary>
       Task<Dictionary<DateOnly, decimal>> GetExchangeRatesAsync(Currency currency, List<DateOnly> dates, CancellationToken cancellationToken = default);
   }
   ```

   Create `src/InvestTax.Core/Interfaces/IEmailService.cs`:
   ```csharp
   namespace InvestTax.Core.Interfaces;
   
   /// <summary>
   /// Email sending service
   /// </summary>
   public interface IEmailService
   {
       Task SendEmailAsync(string toEmail, string subject, string bodyText, CancellationToken cancellationToken = default);
       Task SendTaxReportEmailAsync(string toEmail, string reportContent, string jobId, CancellationToken cancellationToken = default);
       Task SendErrorEmailAsync(string toEmail, string errorMessage, string jobId, CancellationToken cancellationToken = default);
   }
   ```

   Create `src/InvestTax.Core/Interfaces/ITaxCalculator.cs`:
   ```csharp
   using InvestTax.Core.Models;
   
   namespace InvestTax.Core.Interfaces;
   
   /// <summary>
   /// FIFO tax calculation service
   /// </summary>
   public interface ITaxCalculator
   {
       /// <summary>
       /// Calculate taxes for all transactions using FIFO methodology
       /// </summary>
       /// <param name="transactions">All buy and sell transactions</param>
       /// <param name="year">Tax year</param>
       /// <returns>Complete tax summary</returns>
       TaxSummary CalculateTaxes(List<Transaction> transactions, int year);
   }
   ```

5. **Create Custom Exceptions**

   Create `src/InvestTax.Core/Exceptions/ValidationException.cs`:
   ```csharp
   namespace InvestTax.Core.Exceptions;
   
   /// <summary>
   /// Exception thrown when CSV validation fails
   /// </summary>
   public class ValidationException : Exception
   {
       public List<string> ValidationErrors { get; }
       
       public ValidationException(string message, List<string> errors) : base(message)
       {
           ValidationErrors = errors;
       }
       
       public ValidationException(string message) : this(message, new List<string>())
       {
       }
   }
   ```

   Create `src/InvestTax.Core/Exceptions/NBPApiException.cs`:
   ```csharp
   namespace InvestTax.Core.Exceptions;
   
   /// <summary>
   /// Exception thrown when NBP API calls fail
   /// </summary>
   public class NBPApiException : Exception
   {
       public string? Currency { get; set; }
       public DateOnly? RequestedDate { get; set; }
       
       public NBPApiException(string message) : base(message)
       {
       }
       
       public NBPApiException(string message, Exception innerException) : base(message, innerException)
       {
       }
   }
   ```

6. **Build and Verify**
   ```bash
   dotnet build src/InvestTax.Core/InvestTax.Core.csproj
   ```

#### Expected Output
```
Project Structure:
src/InvestTax.Core/
├── Enums/
│   ├── JobStatus.cs
│   ├── TransactionAction.cs
│   └── Currency.cs
├── Models/
│   ├── Transaction.cs
│   ├── TaxCalculation.cs
│   ├── TaxSummary.cs
│   ├── Job.cs
│   ├── NBPExchangeRateResponse.cs
│   └── LambdaInput.cs
├── Interfaces/
│   ├── IS3Service.cs
│   ├── IDynamoDbService.cs
│   ├── INBPApiClient.cs
│   ├── IEmailService.cs
│   └── ITaxCalculator.cs
└── Exceptions/
    ├── ValidationException.cs
    └── NBPApiException.cs
```

#### Success Criteria
- [ ] All models compile without errors
- [ ] All interfaces defined
- [ ] Enums created
- [ ] Custom exceptions created
- [ ] InvestTax.Core project builds successfully

---

## PHASE 2: CORE LAMBDA FUNCTIONS

---

### Step 5: Shared Infrastructure Services

**Duration**: 3-4 hours  
**Dependencies**: Step 4  
**Prerequisites**: Core models created

#### Objective
Implement AWS service wrapper implementations (S3, DynamoDB, SES, NBP API client) in Infrastructure project.

#### Technology Stack
- **AWS SDK**: AWSSDK.S3, AWSSDK.DynamoDBv2, AWSSDK.SimpleEmail 3.7.x
- **HTTP Client**: System.Net.Http with Polly 8.3.x for retry logic
- **JSON**: System.Text.Json

#### Actions

1. **Add NuGet Packages to Infrastructure Project**
   ```bash
   cd src/InvestTax.Infrastructure
   
   dotnet add package AWSSDK.S3 --version 3.7.*
   dotnet add package AWSSDK.DynamoDBv2 --version 3.7.*
   dotnet add package AWSSDK.SimpleEmail --version 3.7.*
   dotnet add package Microsoft.Extensions.Logging.Abstractions --version 8.0.*
   dotnet add package Microsoft.Extensions.Options --version 8.0.*
   dotnet add package Polly --version 8.3.*
   dotnet add package Polly.Extensions.Http --version 3.0.*
   ```

2. **Implement S3Service**

   Create `src/InvestTax.Infrastructure/AWS/S3Service.cs`:
   ```csharp
   using Amazon.S3;
   using Amazon.S3.Model;
   using InvestTax.Core.Interfaces;
   using Microsoft.Extensions.Logging;
   
   namespace InvestTax.Infrastructure.AWS;
   
   public class S3Service : IS3Service
   {
       private readonly IAmazonS3 _s3Client;
       private readonly ILogger<S3Service> _logger;
   
       public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger)
       {
           _s3Client = s3Client;
           _logger = logger;
       }
   
       public async Task<string> GetObjectAsStringAsync(string bucketName, string key, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Getting object s3://{Bucket}/{Key}", bucketName, key);
           
           var request = new GetObjectRequest
           {
               BucketName = bucketName,
               Key = key
           };
   
           using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
           using var reader = new StreamReader(response.ResponseStream);
           return await reader.ReadToEndAsync(cancellationToken);
       }
   
       public async Task<Stream> GetObjectStreamAsync(string bucketName, string key, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Getting object stream s3://{Bucket}/{Key}", bucketName, key);
           
           var request = new GetObjectRequest
           {
               BucketName = bucketName,
               Key = key
           };
   
           var response = await _s3Client.GetObjectAsync(request, cancellationToken);
           return response.ResponseStream;
       }
   
       public async Task PutObjectAsync(string bucketName, string key, string content, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Putting object s3://{Bucket}/{Key}", bucketName, key);
           
           var request = new PutObjectRequest
           {
               BucketName = bucketName,
               Key = key,
               ContentBody = content
           };
   
           await _s3Client.PutObjectAsync(request, cancellationToken);
       }
   
       public async Task PutObjectAsync(string bucketName, string key, Stream stream, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Putting object stream s3://{Bucket}/{Key}", bucketName, key);
           
           var request = new PutObjectRequest
           {
               BucketName = bucketName,
               Key = key,
               InputStream = stream
           };
   
           await _s3Client.PutObjectAsync(request, cancellationToken);
       }
   
       public async Task<bool> ObjectExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default)
       {
           try
           {
               await _s3Client.GetObjectMetadataAsync(bucketName, key, cancellationToken);
               return true;
           }
           catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
           {
               return false;
           }
       }
   
       public async Task DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Deleting object s3://{Bucket}/{Key}", bucketName, key);
           
           var request = new DeleteObjectRequest
           {
               BucketName = bucketName,
               Key = key
           };
   
           await _s3Client.DeleteObjectAsync(request, cancellationToken);
       }
   
       public async Task<Dictionary<string, string>> GetObjectMetadataAsync(string bucketName, string key, CancellationToken cancellationToken = default)
       {
           var request = new GetObjectMetadataRequest
           {
               BucketName = bucketName,
               Key = key
           };
   
           var response = await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
           return response.Metadata.Keys.ToDictionary(k => k, k => response.Metadata[k]);
       }
   }
   ```

3. **Implement DynamoDbService**

   Create `src/InvestTax.Infrastructure/AWS/DynamoDbService.cs`:
   ```csharp
   using Amazon.DynamoDBv2;
   using Amazon.DynamoDBv2.DocumentModel;
   using Amazon.DynamoDBv2.Model;
   using InvestTax.Core.Enums;
   using InvestTax.Core.Interfaces;
   using InvestTax.Core.Models;
   using Microsoft.Extensions.Logging;
   using System.Text.Json;
   
   namespace InvestTax.Infrastructure.AWS;
   
   public class DynamoDbService : IDynamoDbService
   {
       private readonly IAmazonDynamoDB _dynamoDbClient;
       private readonly string _tableName;
       private readonly ILogger<DynamoDbService> _logger;
   
       public DynamoDbService(
           IAmazonDynamoDB dynamoDbClient,
           string tableName,
           ILogger<DynamoDbService> logger)
       {
           _dynamoDbClient = dynamoDbClient;
           _tableName = tableName;
           _logger = logger;
       }
   
       public async Task<Job?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Getting job {JobId} from DynamoDB", jobId);
           
           var request = new GetItemRequest
           {
               TableName = _tableName,
               Key = new Dictionary<string, AttributeValue>
               {
                   { "JobId", new AttributeValue { S = jobId } }
               }
           };
   
           var response = await _dynamoDbClient.GetItemAsync(request, cancellationToken);
           
           if (!response.IsItemSet)
           {
               _logger.LogWarning("Job {JobId} not found", jobId);
               return null;
           }
   
           return MapToJob(response.Item);
       }
   
       public async Task SaveJobAsync(Job job, CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Saving job {JobId} to DynamoDB", job.JobId);
           
           job.UpdatedAt = DateTime.UtcNow.ToString("o");
           
           var request = new PutItemRequest
           {
               TableName = _tableName,
               Item = MapToAttributeValues(job)
           };
   
           await _dynamoDbClient.PutItemAsync(request, cancellationToken);
       }
   
       public async Task UpdateJobStatusAsync(
           string jobId,
           JobStatus status,
           string? errorMessage = null,
           CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Updating job {JobId} status to {Status}", jobId, status);
           
           var updateExpression = "SET #status = :status, #updatedAt = :updatedAt";
           var expressionAttributeNames = new Dictionary<string, string>
           {
               { "#status", "Status" },
               { "#updatedAt", "UpdatedAt" }
           };
           
           var expressionAttributeValues = new Dictionary<string, AttributeValue>
           {
               { ":status", new AttributeValue { S = status.ToString() } },
               { ":updatedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
           };
   
           if (status == JobStatus.Completed)
           {
               updateExpression += ", #completedAt = :completedAt";
               expressionAttributeNames.Add("#completedAt", "CompletedAt");
               expressionAttributeValues.Add(":completedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") });
           }
   
           if (!string.IsNullOrEmpty(errorMessage))
           {
               updateExpression += ", #errorMessage = :errorMessage";
               expressionAttributeNames.Add("#errorMessage", "ErrorMessage");
               expressionAttributeValues.Add(":errorMessage", new AttributeValue { S = errorMessage });
           }
   
           var request = new UpdateItemRequest
           {
               TableName = _tableName,
               Key = new Dictionary<string, AttributeValue>
               {
                   { "JobId", new AttributeValue { S = jobId } }
               },
               UpdateExpression = updateExpression,
               ExpressionAttributeNames = expressionAttributeNames,
               ExpressionAttributeValues = expressionAttributeValues
           };
   
           await _dynamoDbClient.UpdateItemAsync(request, cancellationToken);
       }
   
       public async Task<List<Job>> GetJobsByStatusAsync(
           JobStatus status,
           int limit = 100,
           CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Querying jobs by status {Status}", status);
           
           var request = new QueryRequest
           {
               TableName = _tableName,
               IndexName = "StatusIndex",
               KeyConditionExpression = "#status = :status",
               ExpressionAttributeNames = new Dictionary<string, string>
               {
                   { "#status", "Status" }
               },
               ExpressionAttributeValues = new Dictionary<string, AttributeValue>
               {
                   { ":status", new AttributeValue { S = status.ToString() } }
               },
               Limit = limit,
               ScanIndexForward = false // Most recent first
           };
   
           var response = await _dynamoDbClient.QueryAsync(request, cancellationToken);
           return response.Items.Select(MapToJob).ToList();
       }
   
       private Job MapToJob(Dictionary<string, AttributeValue> item)
       {
           return new Job
           {
               JobId = item["JobId"].S,
               Email = item["Email"].S,
               S3Key = item["S3Key"].S,
               Status = Enum.Parse<JobStatus>(item["Status"].S),
               CreatedAt = item["CreatedAt"].S,
               UpdatedAt = item["UpdatedAt"].S,
               CompletedAt = item.ContainsKey("CompletedAt") ? item["CompletedAt"].S : null,
               ErrorMessage = item.ContainsKey("ErrorMessage") ? item["ErrorMessage"].S : null,
               ExecutionArn = item.ContainsKey("ExecutionArn") ? item["ExecutionArn"].S : null,
               TransactionCount = item.ContainsKey("TransactionCount") && item["TransactionCount"].N != null 
                   ? int.Parse(item["TransactionCount"].N) : null,
               DurationSeconds = item.ContainsKey("DurationSeconds") && item["DurationSeconds"].N != null 
                   ? double.Parse(item["DurationSeconds"].N) : null
           };
       }
   
       private Dictionary<string, AttributeValue> MapToAttributeValues(Job job)
       {
           var item = new Dictionary<string, AttributeValue>
           {
               { "JobId", new AttributeValue { S = job.JobId } },
               { "Email", new AttributeValue { S = job.Email } },
               { "S3Key", new AttributeValue { S = job.S3Key } },
               { "Status", new AttributeValue { S = job.Status.ToString() } },
               { "CreatedAt", new AttributeValue { S = job.CreatedAt } },
               { "UpdatedAt", new AttributeValue { S = job.UpdatedAt } }
           };
   
           if (!string.IsNullOrEmpty(job.CompletedAt))
               item["CompletedAt"] = new AttributeValue { S = job.CompletedAt };
           
           if (!string.IsNullOrEmpty(job.ErrorMessage))
               item["ErrorMessage"] = new AttributeValue { S = job.ErrorMessage };
           
           if (!string.IsNullOrEmpty(job.ExecutionArn))
               item["ExecutionArn"] = new AttributeValue { S = job.ExecutionArn };
           
           if (job.TransactionCount.HasValue)
               item["TransactionCount"] = new AttributeValue { N = job.TransactionCount.Value.ToString() };
           
           if (job.DurationSeconds.HasValue)
               item["DurationSeconds"] = new AttributeValue { N = job.DurationSeconds.Value.ToString("F2") };
   
           return item;
       }
   }
   ```

4. **Implement NBPApiClient with Polly Retry**

   Create `src/InvestTax.Infrastructure/Services/NBPApiClient.cs`:
   ```csharp
   using InvestTax.Core.Enums;
   using InvestTax.Core.Exceptions;
   using InvestTax.Core.Interfaces;
   using InvestTax.Core.Models;
   using Microsoft.Extensions.Logging;
   using Polly;
   using Polly.Retry;
   using System.Net;
   using System.Text.Json;
   
   namespace InvestTax.Infrastructure.Services;
   
   public class NBPApiClient : INBPApiClient
   {
       private readonly HttpClient _httpClient;
       private readonly ILogger<NBPApiClient> _logger;
       private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
   
       public NBPApiClient(HttpClient httpClient, ILogger<NBPApiClient> logger)
       {
           _httpClient = httpClient;
           _logger = logger;
           
           // Configure Polly retry policy: 3 attempts with exponential backoff
           _retryPolicy = Policy
               .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
               .Or<HttpRequestException>()
               .WaitAndRetryAsync(
                   retryCount: 3,
                   sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                   onRetry: (outcome, timespan, retryCount, context) =>
                   {
                       _logger.LogWarning(
                           "NBP API request failed (attempt {RetryCount}/3). Retrying after {Delay}s. Status: {Status}",
                           retryCount,
                           timespan.TotalSeconds,
                           outcome.Result?.StatusCode);
                   });
       }
   
       public async Task<decimal> GetExchangeRateAsync(
           Currency currency,
           DateOnly date,
           CancellationToken cancellationToken = default)
       {
           if (currency == Currency.PLN)
               return 1.0m;
   
           var currencyCode = currency.ToString().ToUpperInvariant();
           var dateStr = date.ToString("yyyy-MM-dd");
           
           // NBP API URL: https://api.nbp.pl/api/exchangerates/rates/a/{currency}/{date}/?format=json
           var url = $"exchangerates/rates/a/{currencyCode}/{dateStr}/?format=json";
           
           _logger.LogInformation("Fetching NBP rate for {Currency} on {Date}", currencyCode, dateStr);
   
           try
           {
               var response = await _retryPolicy.ExecuteAsync(async () =>
               {
                   var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
                   return httpResponse;
               });
   
               if (!response.IsSuccessStatusCode)
               {
                   if (response.StatusCode == HttpStatusCode.NotFound)
                   {
                       throw new NBPApiException($"Exchange rate not available for {currencyCode} on {dateStr}. Date may be a weekend or holiday.")
                       {
                           Currency = currencyCode,
                           RequestedDate = date
                       };
                   }
                   
                   throw new NBPApiException($"NBP API returned status {response.StatusCode}")
                   {
                       Currency = currencyCode,
                       RequestedDate = date
                   };
               }
   
               var content = await response.Content.ReadAsStringAsync(cancellationToken);
               var rateResponse = JsonSerializer.Deserialize<NBPExchangeRateResponse>(content, new JsonSerializerOptions
               {
                   PropertyNameCaseInsensitive = true
               });
   
               if (rateResponse?.Rates == null || !rateResponse.Rates.Any())
               {
                   throw new NBPApiException($"Invalid response from NBP API for {currencyCode} on {dateStr}");
               }
   
               var rate = rateResponse.Rates.First().Mid;
               _logger.LogInformation("Retrieved NBP rate: 1 {Currency} = {Rate} PLN", currencyCode, rate);
               
               return rate;
           }
           catch (NBPApiException)
           {
               throw;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Failed to fetch NBP rate for {Currency} on {Date}", currencyCode, dateStr);
               throw new NBPApiException($"Failed to fetch exchange rate: {ex.Message}", ex)
               {
                   Currency = currencyCode,
                   RequestedDate = date
               };
           }
       }
   
       public async Task<Dictionary<DateOnly, decimal>> GetExchangeRatesAsync(
           Currency currency,
           List<DateOnly> dates,
           CancellationToken cancellationToken = default)
       {
           var result = new Dictionary<DateOnly, decimal>();
           
           // MVP: Sequential requests (Phase 2 will add parallelization)
           foreach (var date in dates.Distinct())
           {
               var rate = await GetExchangeRateAsync(currency, date, cancellationToken);
               result[date] = rate;
           }
   
           return result;
       }
   }
   ```

5. **Implement EmailService**

   Create `src/InvestTax.Infrastructure/AWS/EmailService.cs`:
   ```csharp
   using Amazon.SimpleEmail;
   using Amazon.SimpleEmail.Model;
   using InvestTax.Core.Interfaces;
   using Microsoft.Extensions.Logging;
   
   namespace InvestTax.Infrastructure.AWS;
   
   public class EmailService : IEmailService
   {
       private readonly IAmazonSimpleEmailService _sesClient;
       private readonly string _fromEmail;
       private readonly ILogger<EmailService> _logger;
   
       public EmailService(
           IAmazonSimpleEmailService sesClient,
           string fromEmail,
           ILogger<EmailService> logger)
       {
           _sesClient = sesClient;
           _fromEmail = fromEmail;
           _logger = logger;
       }
   
       public async Task SendEmailAsync(
           string toEmail,
           string subject,
           string bodyText,
           CancellationToken cancellationToken = default)
       {
           _logger.LogInformation("Sending email to {Email}: {Subject}", toEmail, subject);
           
           var request = new SendEmailRequest
           {
               Source = _fromEmail,
               Destination = new Destination
               {
                   ToAddresses = new List<string> { toEmail }
               },
               Message = new Message
               {
                   Subject = new Content(subject),
                   Body = new Body
                   {
                       Text = new Content(bodyText)
                   }
               }
           };
   
           try
           {
               var response = await _sesClient.SendEmailAsync(request, cancellationToken);
               _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", response.MessageId);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
               throw;
           }
       }
   
       public async Task SendTaxReportEmailAsync(
           string toEmail,
           string reportContent,
           string jobId,
           CancellationToken cancellationToken = default)
       {
           var subject = $"InvestTax Calculator - Your Tax Report (Job: {jobId})";
           var body = $@"
   Your tax calculation has completed successfully.
   
   {reportContent}
   
   ---
   Disclaimer: This calculation is for informational purposes only. Please consult with a tax professional before filing.
   
   Job ID: {jobId}
   Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
   ";
   
           await SendEmailAsync(toEmail, subject, body, cancellationToken);
       }
   
       public async Task SendErrorEmailAsync(
           string toEmail,
           string errorMessage,
           string jobId,
           CancellationToken cancellationToken = default)
       {
           var subject = $"InvestTax Calculator - Processing Error (Job: {jobId})";
           var body = $@"
   We encountered an error processing your tax calculation.
   
   Error: {errorMessage}
   
   Please verify your CSV file format and try again. If the problem persists, contact support.
   
   Job ID: {jobId}
   Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
   ";
   
           await SendEmailAsync(toEmail, subject, body, cancellationToken);
       }
   }
   ```

6. **Build Infrastructure Project**
   ```bash
   dotnet build src/InvestTax.Infrastructure/InvestTax.Infrastructure.csproj
   ```

#### Expected Output
```
Infrastructure Services Implemented:
- S3Service (IS3Service)
- DynamoDbService (IDynamoDbService)
- NBPApiClient (INBPApiClient) with Polly retry
- EmailService (IEmailService)

All services integrate with AWS SDK and include:
- Logging
- Error handling
- Cancellation token support
```

#### Success Criteria
- [ ] All services implement their respective interfaces
- [ ] Project builds without errors
- [ ] AWS SDK packages installed
- [ ] Polly retry configured for NBP API
- [ ] Logging integrated

---

### Step 6: CSV Validator Lambda Implementation

**Duration**: 3-4 hours  
**Dependencies**: Step 5  
**Prerequisites**: Shared infrastructure services implemented

#### Objective
Implement Lambda function to validate CSV file structure, headers, data types, and business rules before processing.

#### Technology Stack
- **.NET Runtime**: 10.0
- **CSV Parsing**: CsvHelper 33.x
- **AWS SDK**: AWSSDK.S3, AWSSDK.Lambda
- **Validation**: FluentValidation 11.9.x
- **Logging**: AWS.Lambda.Logging.AspNetCore 4.x

#### Actions

1. **Add NuGet Packages to Validator Project**
   ```bash
   cd src/InvestTax.Lambda.Validator
   dotnet add package Amazon.Lambda.Core --version 2.2.0
   dotnet add package Amazon.Lambda.Serialization.SystemTextJson --version 2.4.1
   dotnet add package AWSSDK.S3 --version 3.7.307
   dotnet add package CsvHelper --version 33.0.1
   dotnet add package FluentValidation --version 11.9.0
   dotnet add package AWS.Lambda.Logging.AspNetCore --version 4.1.0
   ```

2. **Create Validation Models**

   Create `Models/CsvRow.cs`:
   ```csharp
   namespace InvestTax.Lambda.Validator.Models;
   
   public class CsvRow
   {
       public string Action { get; set; }
       public string Time { get; set; }
       public string ISIN { get; set; }
       public string Ticker { get; set; }
       public string Name { get; set; }
       public string NoOfShares { get; set; }
       public string PricePerShare { get; set; }
       public string CurrencySymbol { get; set; }
       public string ExchangeRate { get; set; }
       public string Result { get; set; }
       public string Total { get; set; }
       public string Notes { get; set; }
   }
   ```

   Create `Models/ValidationResult.cs`:
   ```csharp
   namespace InvestTax.Lambda.Validator.Models;
   
   public class ValidationResult
   {
       public bool Valid { get; set; }
       public int RowCount { get; set; }
       public int Year { get; set; }
       public List<string> Currencies { get; set; } = new();
       public string ValidatedFileKey { get; set; }
       public List<ValidationError> Errors { get; set; } = new();
   }
   
   public class ValidationError
   {
       public int Row { get; set; }
       public string Column { get; set; }
       public string Message { get; set; }
   }
   ```

3. **Create CSV Row Validator**

   Create `Validators/CsvRowValidator.cs`:
   ```csharp
   using FluentValidation;
   using InvestTax.Lambda.Validator.Models;
   
   namespace InvestTax.Lambda.Validator.Validators;
   
   public class CsvRowValidator : AbstractValidator<CsvRow>
   {
       private static readonly string[] ValidActions = { "Market buy", "Market sell" };
       
       public CsvRowValidator()
       {
           RuleFor(x => x.Action)
               .NotEmpty()
               .Must(a => ValidActions.Contains(a))
               .WithMessage("Action must be 'Market buy' or 'Market sell'");
           
           RuleFor(x => x.Time)
               .NotEmpty()
               .Must(BeValidDateTime)
               .WithMessage("Time must be valid ISO 8601 datetime");
           
           RuleFor(x => x.ISIN)
               .NotEmpty()
               .Length(12)
               .Matches("^[A-Z0-9]{12}$")
               .WithMessage("ISIN must be 12 alphanumeric characters");
           
           RuleFor(x => x.NoOfShares)
               .NotEmpty()
               .Must(BePositiveDecimal)
               .WithMessage("No. of shares must be positive number");
           
           RuleFor(x => x.PricePerShare)
               .NotEmpty()
               .Must(BePositiveDecimal)
               .WithMessage("Price per share must be positive number");
           
           RuleFor(x => x.CurrencySymbol)
               .NotEmpty()
               .Length(3)
               .Matches("^[A-Z]{3}$")
               .WithMessage("Currency must be 3-letter ISO code");
       }
       
       private bool BeValidDateTime(string dateTime)
       {
           return DateTime.TryParse(dateTime, out _);
       }
       
       private bool BePositiveDecimal(string value)
       {
           return decimal.TryParse(value, out var result) && result > 0;
       }
   }
   ```

4. **Create Validation Service**

   Create `Services/CsvValidationService.cs`:
   ```csharp
   using CsvHelper;
   using CsvHelper.Configuration;
   using InvestTax.Lambda.Validator.Models;
   using InvestTax.Lambda.Validator.Validators;
   using System.Globalization;
   
   namespace InvestTax.Lambda.Validator.Services;
   
   public class CsvValidationService
   {
       private readonly CsvRowValidator _validator;
       private readonly ILogger<CsvValidationService> _logger;
       
       public CsvValidationService(ILogger<CsvValidationService> logger)
       {
           _validator = new CsvRowValidator();
           _logger = logger;
       }
       
       public async Task<ValidationResult> ValidateFileAsync(
           string filePath, 
           CancellationToken cancellationToken)
       {
           var result = new ValidationResult();
           var currencies = new HashSet<string>();
           int? year = null;
           
           try
           {
               var config = new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   Delimiter = "|",
                   HasHeaderRecord = true,
                   TrimOptions = TrimOptions.Trim
               };
               
               using var reader = new StreamReader(filePath);
               using var csv = new CsvReader(reader, config);
               
               var records = csv.GetRecords<CsvRow>().ToList();
               
               if (records.Count == 0)
               {
                   result.Errors.Add(new ValidationError
                   {
                       Row = 0,
                       Column = "File",
                       Message = "File contains no data rows"
                   });
                   return result;
               }
               
               if (records.Count > 100000)
               {
                   result.Errors.Add(new ValidationError
                   {
                       Row = 0,
                       Column = "File",
                       Message = "File exceeds maximum 100,000 rows"
                   });
                   return result;
               }
               
               for (int i = 0; i < records.Count; i++)
               {
                   var record = records[i];
                   var rowNumber = i + 2; // Account for header row
                   
                   var validationResult = _validator.Validate(record);
                   
                   if (!validationResult.IsValid)
                   {
                       foreach (var error in validationResult.Errors)
                       {
                           result.Errors.Add(new ValidationError
                           {
                               Row = rowNumber,
                               Column = error.PropertyName,
                               Message = error.ErrorMessage
                           });
                       }
                   }
                   
                   // Extract year from first valid date
                   if (year == null && DateTime.TryParse(record.Time, out var dt))
                   {
                       year = dt.Year;
                   }
                   
                   // Collect unique currencies
                   if (!string.IsNullOrEmpty(record.CurrencySymbol))
                   {
                       currencies.Add(record.CurrencySymbol.ToUpper());
                   }
               }
               
               result.Valid = result.Errors.Count == 0;
               result.RowCount = records.Count;
               result.Year = year ?? DateTime.Now.Year;
               result.Currencies = currencies.ToList();
               
               _logger.LogInformation(
                   "Validation complete: {Valid}, Rows: {RowCount}, Errors: {ErrorCount}",
                   result.Valid, result.RowCount, result.Errors.Count);
               
               return result;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error validating CSV file");
               result.Errors.Add(new ValidationError
               {
                   Row = 0,
                   Column = "File",
                   Message = $"File parsing error: {ex.Message}"
               });
               return result;
           }
       }
   }
   ```

5. **Implement Lambda Function Handler**

   Update `Function.cs`:
   ```csharp
   using Amazon.Lambda.Core;
   using Amazon.S3;
   using InvestTax.Core.Models;
   using InvestTax.Infrastructure.AWS;
   using InvestTax.Lambda.Validator.Services;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   using System.Text.Json;
   
   [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
   
   namespace InvestTax.Lambda.Validator;
   
   public class Function
   {
       private readonly IS3Service _s3Service;
       private readonly CsvValidationService _validationService;
       private readonly ILogger<Function> _logger;
       
       public Function()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           var serviceProvider = services.BuildServiceProvider();
           
           _s3Service = serviceProvider.GetRequiredService<IS3Service>();
           _validationService = serviceProvider.GetRequiredService<CsvValidationService>();
           _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
       }
       
       private void ConfigureServices(IServiceCollection services)
       {
           services.AddLogging(builder =>
           {
               builder.AddConsole();
               builder.AddLambdaLogger();
           });
           
           services.AddAWSService<IAmazonS3>();
           services.AddSingleton<IS3Service, S3Service>();
           services.AddSingleton<CsvValidationService>();
       }
       
       public async Task<LambdaInput> FunctionHandler(
           LambdaInput input, 
           ILambdaContext context)
       {
           _logger.LogInformation(
               "Starting validation for JobId: {JobId}, File: {FileKey}",
               input.JobId, input.FileKey);
           
           try
           {
               var uploadBucket = Environment.GetEnvironmentVariable("UPLOAD_BUCKET");
               var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET");
               
               // Download file from S3
               var localPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}.csv");
               await _s3Service.DownloadFileAsync(
                   uploadBucket, 
                   input.FileKey, 
                   localPath, 
                   context.CancellationToken);
               
               // Validate file
               var validationResult = await _validationService.ValidateFileAsync(
                   localPath, 
                   context.CancellationToken);
               
               if (validationResult.Valid)
               {
                   // Upload validated file to processing bucket
                   var validatedKey = $"validated/{input.JobId}.csv";
                   await _s3Service.UploadFileAsync(
                       processingBucket, 
                       validatedKey, 
                       localPath, 
                       context.CancellationToken);
                   
                   input.ValidatedFileKey = validatedKey;
                   input.RowCount = validationResult.RowCount;
                   input.Year = validationResult.Year;
                   input.Currencies = validationResult.Currencies;
                   input.Stage = "VALIDATED";
                   
                   _logger.LogInformation(
                       "Validation successful: {RowCount} rows, Year: {Year}",
                       validationResult.RowCount, validationResult.Year);
               }
               else
               {
                   input.Stage = "VALIDATION_FAILED";
                   input.ErrorMessage = JsonSerializer.Serialize(validationResult.Errors);
                   
                   _logger.LogWarning(
                       "Validation failed with {ErrorCount} errors",
                       validationResult.Errors.Count);
               }
               
               // Cleanup local file
               if (File.Exists(localPath))
               {
                   File.Delete(localPath);
               }
               
               return input;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error in validator function");
               input.Stage = "VALIDATION_ERROR";
               input.ErrorMessage = ex.Message;
               throw;
           }
       }
   }
   ```

6. **Create Lambda Configuration**

   Update `aws-lambda-tools-defaults.json`:
   ```json
   {
     "Information": [
       "This file provides default values for the deployment wizard inside Visual Studio and the AWS Lambda commands added to the .NET Core CLI."
     ],
     "profile": "",
     "region": "eu-central-1",
     "configuration": "Release",
     "function-runtime": "dotnet10",
     "function-memory-size": 512,
     "function-timeout": 300,
     "function-handler": "InvestTax.Lambda.Validator::InvestTax.Lambda.Validator.Function::FunctionHandler",
     "environment-variables": {
       "UPLOAD_BUCKET": "investtax-upload-dev",
       "PROCESSING_BUCKET": "investtax-processing-dev"
     }
   }
   ```

7. **Build and Test Locally**
   ```bash
   cd src/InvestTax.Lambda.Validator
   dotnet build
   dotnet lambda test-tool
   ```

#### Expected Output
```
Lambda Function Structure:
InvestTax.Lambda.Validator/
├── Function.cs
├── Models/
│   ├── CsvRow.cs
│   └── ValidationResult.cs
├── Validators/
│   └── CsvRowValidator.cs
├── Services/
│   └── CsvValidationService.cs
└── aws-lambda-tools-defaults.json

Validation Result (Success):
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "stage": "VALIDATED",
  "validatedFileKey": "validated/550e8400-e29b-41d4-a716-446655440000.csv",
  "rowCount": 234,
  "year": 2024,
  "currencies": ["USD", "EUR"]
}

Validation Result (Failure):
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "stage": "VALIDATION_FAILED",
  "errorMessage": "[{\"row\":15,\"column\":\"Time\",\"message\":\"Invalid date\"}]"
}
```

#### Success Criteria
- [ ] Lambda function builds without errors
- [ ] CsvHelper successfully parses pipe-delimited files
- [ ] FluentValidation rules validate all required fields
- [ ] Valid files uploaded to processing bucket
- [ ] Validation errors returned in structured format
- [ ] Local testing with dotnet lambda test-tool works

#### Troubleshooting
- **CsvHelper parsing errors**: Check delimiter configuration (pipe vs comma)
- **Memory errors**: Increase function memory if processing large files
- **S3 access denied**: Verify Lambda execution role has S3 permissions
- **Missing headers**: Check CsvHelper header mapping configuration

---

### Step 7: Data Normalizer Lambda Implementation

**Duration**: 3-4 hours  
**Dependencies**: Step 6  
**Prerequisites**: Validator Lambda functional

#### Objective
Implement Lambda function to normalize dates, currencies, numbers, and group transactions by ISIN for downstream processing.

#### Technology Stack
- **.NET Runtime**: 10.0
- **CSV Parsing**: CsvHelper 33.x
- **JSON Serialization**: System.Text.Json
- **Date/Time**: NodaTime 3.1.x (timezone handling)

#### Actions

1. **Add NuGet Packages**
   ```bash
   cd src/InvestTax.Lambda.Normalizer
   dotnet add package NodaTime --version 3.1.9
   dotnet add package CsvHelper --version 33.0.1
   ```

2. **Create Normalized Transaction Models**

   Create `Models/NormalizedTransaction.cs`:
   ```csharp
   namespace InvestTax.Lambda.Normalizer.Models;
   
   public class NormalizedTransaction
   {
       public int Id { get; set; }
       public TransactionAction Action { get; set; }
       public DateTime TransactionDate { get; set; }
       public string ISIN { get; set; }
       public string Ticker { get; set; }
       public string Name { get; set; }
       public decimal Shares { get; set; }
       public decimal PricePerShare { get; set; }
       public string Currency { get; set; }
       public decimal ExchangeRate { get; set; }
       public decimal Total { get; set; }
       public string Notes { get; set; }
   }
   
   public class TransactionGroup
   {
       public string ISIN { get; set; }
       public string Ticker { get; set; }
       public List<NormalizedTransaction> Transactions { get; set; } = new();
   }
   
   public class NormalizationResult
   {
       public string NormalizedFileKey { get; set; }
       public Dictionary<string, TransactionGroup> TransactionGroups { get; set; } = new();
       public int TotalTransactions { get; set; }
   }
   ```

3. **Create Normalization Service**

   Create `Services/NormalizationService.cs`:
   ```csharp
   using CsvHelper;
   using CsvHelper.Configuration;
   using InvestTax.Lambda.Normalizer.Models;
   using NodaTime;
   using System.Globalization;
   using System.Text.Json;
   
   namespace InvestTax.Lambda.Normalizer.Services;
   
   public class NormalizationService
   {
       private readonly ILogger<NormalizationService> _logger;
       private readonly DateTimeZone _warsawTimeZone;
       
       public NormalizationService(ILogger<NormalizationService> logger)
       {
           _logger = logger;
           _warsawTimeZone = DateTimeZoneProviders.Tzdb["Europe/Warsaw"];
       }
       
       public async Task<NormalizationResult> NormalizeAsync(
           string inputPath,
           string outputPath,
           CancellationToken cancellationToken)
       {
           var result = new NormalizationResult();
           var transactions = new List<NormalizedTransaction>();
           
           try
           {
               // Read and parse CSV
               var config = new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   Delimiter = "|",
                   HasHeaderRecord = true,
                   TrimOptions = TrimOptions.Trim
               };
               
               using (var reader = new StreamReader(inputPath))
               using (var csv = new CsvReader(reader, config))
               {
                   var records = csv.GetRecords<dynamic>().ToList();
                   int id = 1;
                   
                   foreach (var record in records)
                   {
                       var normalized = NormalizeRow(record, id++);
                       transactions.Add(normalized);
                   }
               }
               
               // Sort by date (earliest first)
               transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
               
               // Group by ISIN
               var grouped = transactions.GroupBy(t => t.ISIN);
               
               foreach (var group in grouped)
               {
                   result.TransactionGroups[group.Key] = new TransactionGroup
                   {
                       ISIN = group.Key,
                       Ticker = group.First().Ticker,
                       Transactions = group.OrderBy(t => t.TransactionDate).ToList()
                   };
               }
               
               result.TotalTransactions = transactions.Count;
               
               // Write JSON output
               var json = JsonSerializer.Serialize(result.TransactionGroups, new JsonSerializerOptions
               {
                   WriteIndented = true
               });
               
               await File.WriteAllTextAsync(outputPath, json, cancellationToken);
               
               _logger.LogInformation(
                   "Normalized {Count} transactions into {Groups} ISIN groups",
                   result.TotalTransactions, result.TransactionGroups.Count);
               
               return result;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error normalizing data");
               throw;
           }
       }
       
       private NormalizedTransaction NormalizeRow(dynamic record, int id)
       {
           var transaction = new NormalizedTransaction
           {
               Id = id,
               Action = ParseAction(record.Action),
               TransactionDate = ParseDate(record.Time),
               ISIN = ((string)record.ISIN).Trim().ToUpper(),
               Ticker = ((string)record.Ticker).Trim().ToUpper(),
               Name = ((string)record.Name).Trim(),
               Shares = ParseDecimal(record.NoOfShares),
               PricePerShare = ParseDecimal(record.PricePerShare),
               Currency = ((string)record.CurrencySymbol).Trim().ToUpper(),
               ExchangeRate = ParseDecimal(record.ExchangeRate),
               Total = ParseDecimal(record.Total),
               Notes = record.Notes?.ToString()?.Trim() ?? string.Empty
           };
           
           return transaction;
       }
       
       private TransactionAction ParseAction(string action)
       {
           return action.ToLower() switch
           {
               "market buy" => TransactionAction.Buy,
               "market sell" => TransactionAction.Sell,
               _ => throw new InvalidOperationException($"Unknown action: {action}")
           };
       }
       
       private DateTime ParseDate(string dateStr)
       {
           var parsedDate = DateTime.Parse(dateStr);
           
           // Convert to Warsaw timezone
           var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc));
           var zonedDateTime = instant.InZone(_warsawTimeZone);
           
           return zonedDateTime.ToDateTimeUnspecified();
       }
       
       private decimal ParseDecimal(string value)
       {
           // Remove any thousand separators and normalize
           value = value.Replace(",", "").Replace(" ", "").Trim();
           return decimal.Parse(value, CultureInfo.InvariantCulture);
       }
   }
   ```

4. **Implement Lambda Handler**

   Update `Function.cs`:
   ```csharp
   using Amazon.Lambda.Core;
   using Amazon.S3;
   using InvestTax.Core.Models;
   using InvestTax.Infrastructure.AWS;
   using InvestTax.Lambda.Normalizer.Services;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   
   [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
   
   namespace InvestTax.Lambda.Normalizer;
   
   public class Function
   {
       private readonly IS3Service _s3Service;
       private readonly NormalizationService _normalizationService;
       private readonly ILogger<Function> _logger;
       
       public Function()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           var serviceProvider = services.BuildServiceProvider();
           
           _s3Service = serviceProvider.GetRequiredService<IS3Service>();
           _normalizationService = serviceProvider.GetRequiredService<NormalizationService>();
           _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
       }
       
       private void ConfigureServices(IServiceCollection services)
       {
           services.AddLogging(builder =>
           {
               builder.AddConsole();
               builder.AddLambdaLogger();
           });
           
           services.AddAWSService<IAmazonS3>();
           services.AddSingleton<IS3Service, S3Service>();
           services.AddSingleton<NormalizationService>();
       }
       
       public async Task<LambdaInput> FunctionHandler(
           LambdaInput input,
           ILambdaContext context)
       {
           _logger.LogInformation(
               "Starting normalization for JobId: {JobId}",
               input.JobId);
           
           try
           {
               var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET");
               
               // Download validated file
               var inputPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_input.csv");
               await _s3Service.DownloadFileAsync(
                   processingBucket,
                   input.ValidatedFileKey,
                   inputPath,
                   context.CancellationToken);
               
               // Normalize data
               var outputPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_normalized.json");
               var result = await _normalizationService.NormalizeAsync(
                   inputPath,
                   outputPath,
                   context.CancellationToken);
               
               // Upload normalized file
               var normalizedKey = $"normalized/{input.JobId}.json";
               await _s3Service.UploadFileAsync(
                   processingBucket,
                   normalizedKey,
                   outputPath,
                   context.CancellationToken);
               
               input.NormalizedFileKey = normalizedKey;
               input.Stage = "NORMALIZED";
               
               _logger.LogInformation(
                   "Normalization complete: {TotalTransactions} transactions",
                   result.TotalTransactions);
               
               // Cleanup
               File.Delete(inputPath);
               File.Delete(outputPath);
               
               return input;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error in normalizer function");
               input.Stage = "NORMALIZATION_ERROR";
               input.ErrorMessage = ex.Message;
               throw;
           }
       }
   }
   ```

5. **Configure Lambda Settings**

   Update `aws-lambda-tools-defaults.json`:
   ```json
   {
     "profile": "",
     "region": "eu-central-1",
     "configuration": "Release",
     "function-runtime": "dotnet10",
     "function-memory-size": 1024,
     "function-timeout": 600,
     "function-handler": "InvestTax.Lambda.Normalizer::InvestTax.Lambda.Normalizer.Function::FunctionHandler",
     "environment-variables": {
       "PROCESSING_BUCKET": "investtax-processing-dev"
     }
   }
   ```

6. **Build and Test**
   ```bash
   cd src/InvestTax.Lambda.Normalizer
   dotnet build
   ```

#### Expected Output
```
Normalized JSON Structure:
{
  "US0378331005": {
    "isin": "US0378331005",
    "ticker": "AAPL",
    "transactions": [
      {
        "id": 1,
        "action": "Buy",
        "transactionDate": "2024-03-15T10:30:00",
        "isin": "US0378331005",
        "ticker": "AAPL",
        "shares": 10,
        "pricePerShare": 170.50,
        "currency": "USD",
        "total": 1705.00
      }
    ]
  }
}
```

#### Success Criteria
- [ ] CSV successfully parsed and converted to JSON
- [ ] Dates normalized to Europe/Warsaw timezone
- [ ] Numbers converted to decimal (no precision loss)
- [ ] Currencies uppercased and validated
- [ ] Transactions sorted by date
- [ ] Transactions grouped by ISIN
- [ ] JSON output uploaded to S3

#### Troubleshooting
- **Timezone conversion errors**: Verify NodaTime package installed
- **Decimal parsing errors**: Check culture info (use InvariantCulture)
- **Grouping issues**: Verify ISIN uppercase normalization
- **Large file processing**: Increase Lambda memory and timeout

---

### Step 8: NBP Rate Fetcher Lambda Implementation

**Duration**: 3-4 hours  
**Dependencies**: Step 7  
**Prerequisites**: Normalizer Lambda functional

#### Objective
Implement Lambda function to fetch PLN exchange rates from NBP API for all transaction dates and currencies (MVP: no caching, Phase 2 adds DynamoDB cache).

#### Technology Stack
- **.NET Runtime**: 10.0
- **HTTP Client**: System.Net.Http
- **Retry Policy**: Polly 8.3.x
- **JSON**: System.Text.Json

#### Actions

1. **Add NuGet Packages**
   ```bash
   cd src/InvestTax.Lambda.NBPClient
   dotnet add package Polly --version 8.3.0
   dotnet add package Polly.Extensions.Http --version 3.0.0
   ```

2. **Create NBP Models**

   Create `Models/NBPModels.cs`:
   ```csharp
   namespace InvestTax.Lambda.NBPClient.Models;
   
   public class NBPRateRequest
   {
       public string Currency { get; set; }
       public DateTime Date { get; set; }
   }
   
   public class NBPRateResponse
   {
       public string Currency { get; set; }
       public DateTime Date { get; set; }
       public decimal Rate { get; set; }
       public string Source { get; set; } // "NBP_API" or "CACHE"
   }
   
   public class NBPApiResponse
   {
       public string Table { get; set; }
       public string Currency { get; set; }
       public string Code { get; set; }
       public List<NBPRate> Rates { get; set; }
   }
   
   public class NBPRate
   {
       public string No { get; set; }
       public DateTime EffectiveDate { get; set; }
       public decimal Mid { get; set; }
   }
   
   public class RateMap
   {
       public Dictionary<string, decimal> Rates { get; set; } = new();
       // Key format: "{Currency}#{Date:yyyy-MM-dd}"
   }
   ```

3. **Create NBP API Client Service**

   Create `Services/NBPApiService.cs`:
   ```csharp
   using InvestTax.Lambda.NBPClient.Models;
   using Polly;
   using Polly.Extensions.Http;
   using System.Text.Json;
   
   namespace InvestTax.Lambda.NBPClient.Services;
   
   public class NBPApiService
   {
       private readonly HttpClient _httpClient;
       private readonly ILogger<NBPApiService> _logger;
       private const string NBP_BASE_URL = "https://api.nbp.pl/api";
       
       public NBPApiService(ILogger<NBPApiService> logger)
       {
           _logger = logger;
           
           var retryPolicy = HttpPolicyExtensions
               .HandleTransientHttpError()
               .WaitAndRetryAsync(
                   retryCount: 3,
                   sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                   onRetry: (outcome, timespan, retryCount, context) =>
                   {
                       _logger.LogWarning(
                           "NBP API call failed. Retry {RetryCount} after {Delay}ms",
                           retryCount, timespan.TotalMilliseconds);
                   });
           
           _httpClient = new HttpClient();
           _httpClient.BaseAddress = new Uri(NBP_BASE_URL);
           _httpClient.Timeout = TimeSpan.FromSeconds(30);
       }
       
       public async Task<NBPRateResponse> GetExchangeRateAsync(
           string currency,
           DateTime date,
           CancellationToken cancellationToken)
       {
           try
           {
               // NBP API format: /exchangerates/rates/a/{currency}/{date}
               var dateStr = date.ToString("yyyy-MM-dd");
               var url = $"/exchangerates/rates/a/{currency.ToLower()}/{dateStr}/?format=json";
               
               _logger.LogInformation("Fetching NBP rate: {Currency} for {Date}", currency, dateStr);
               
               var response = await _httpClient.GetAsync(url, cancellationToken);
               
               if (!response.IsSuccessStatusCode)
               {
                   if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                   {
                       _logger.LogWarning(
                           "NBP rate not found for {Currency} on {Date} (likely weekend/holiday)",
                           currency, dateStr);
                       
                       // Try previous day (handle weekends/holidays)
                       return await GetExchangeRateAsync(currency, date.AddDays(-1), cancellationToken);
                   }
                   
                   throw new HttpRequestException(
                       $"NBP API returned {response.StatusCode} for {currency} on {dateStr}");
               }
               
               var content = await response.Content.ReadAsStringAsync(cancellationToken);
               var nbpResponse = JsonSerializer.Deserialize<NBPApiResponse>(content);
               
               if (nbpResponse?.Rates == null || nbpResponse.Rates.Count == 0)
               {
                   throw new InvalidOperationException($"No rates returned for {currency} on {dateStr}");
               }
               
               var rate = nbpResponse.Rates[0].Mid;
               
               _logger.LogInformation(
                   "Fetched rate: 1 {Currency} = {Rate} PLN on {Date}",
                   currency, rate, dateStr);
               
               return new NBPRateResponse
               {
                   Currency = currency.ToUpper(),
                   Date = date,
                   Rate = rate,
                   Source = "NBP_API"
               };
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error fetching NBP rate for {Currency} on {Date}", currency, date);
               throw;
           }
       }
       
       public async Task<RateMap> GetRatesForTransactionsAsync(
           Dictionary<string, TransactionGroup> transactionGroups,
           CancellationToken cancellationToken)
       {
           var rateMap = new RateMap();
           var uniqueRequests = new HashSet<(string Currency, DateTime Date)>();
           
           // Extract unique currency-date pairs
           foreach (var group in transactionGroups.Values)
           {
               foreach (var transaction in group.Transactions)
               {
                   // Skip PLN transactions (rate = 1.0)
                   if (transaction.Currency == "PLN")
                       continue;
                   
                   uniqueRequests.Add((transaction.Currency, transaction.TransactionDate.Date));
               }
           }
           
           _logger.LogInformation(
               "Fetching {Count} unique exchange rates from NBP API",
               uniqueRequests.Count);
           
           // Fetch rates sequentially (MVP - Phase 2 will parallelize)
           foreach (var (currency, date) in uniqueRequests)
           {
               try
               {
                   var rateResponse = await GetExchangeRateAsync(currency, date, cancellationToken);
                   var key = $"{currency}#{date:yyyy-MM-dd}";
                   rateMap.Rates[key] = rateResponse.Rate;
               }
               catch (Exception ex)
               {
                   _logger.LogError(
                       ex,
                       "Failed to fetch rate for {Currency} on {Date}",
                       currency, date);
                   throw new InvalidOperationException(
                       $"Unable to fetch exchange rate for {currency} on {date:yyyy-MM-dd}. " +
                       $"This rate is required for tax calculation.");
               }
           }
           
           _logger.LogInformation("Successfully fetched all {Count} rates", rateMap.Rates.Count);
           
           return rateMap;
       }
   }
   ```

4. **Implement Lambda Handler**

   Update `Function.cs`:
   ```csharp
   using Amazon.Lambda.Core;
   using Amazon.S3;
   using InvestTax.Core.Models;
   using InvestTax.Infrastructure.AWS;
   using InvestTax.Lambda.NBPClient.Services;
   using InvestTax.Lambda.NBPClient.Models;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   using System.Text.Json;
   
   [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
   
   namespace InvestTax.Lambda.NBPClient;
   
   public class Function
   {
       private readonly IS3Service _s3Service;
       private readonly NBPApiService _nbpApiService;
       private readonly ILogger<Function> _logger;
       
       public Function()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           var serviceProvider = services.BuildServiceProvider();
           
           _s3Service = serviceProvider.GetRequiredService<IS3Service>();
           _nbpApiService = serviceProvider.GetRequiredService<NBPApiService>();
           _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
       }
       
       private void ConfigureServices(IServiceCollection services)
       {
           services.AddLogging(builder =>
           {
               builder.AddConsole();
               builder.AddLambdaLogger();
           });
           
           services.AddAWSService<IAmazonS3>();
           services.AddSingleton<IS3Service, S3Service>();
           services.AddSingleton<NBPApiService>();
       }
       
       public async Task<LambdaInput> FunctionHandler(
           LambdaInput input,
           ILambdaContext context)
       {
           _logger.LogInformation("Starting NBP rate fetch for JobId: {JobId}", input.JobId);
           
           try
           {
               var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET");
               
               // Download normalized transactions
               var normalizedPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_normalized.json");
               await _s3Service.DownloadFileAsync(
                   processingBucket,
                   input.NormalizedFileKey,
                   normalizedPath,
                   context.CancellationToken);
               
               // Parse transactions
               var json = await File.ReadAllTextAsync(normalizedPath, context.CancellationToken);
               var transactionGroups = JsonSerializer.Deserialize<Dictionary<string, TransactionGroup>>(json);
               
               // Fetch NBP rates
               var rateMap = await _nbpApiService.GetRatesForTransactionsAsync(
                   transactionGroups,
                   context.CancellationToken);
               
               // Save rate map to S3
               var rateMapPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_rates.json");
               var rateMapJson = JsonSerializer.Serialize(rateMap, new JsonSerializerOptions
               {
                   WriteIndented = true
               });
               await File.WriteAllTextAsync(rateMapPath, rateMapJson, context.CancellationToken);
               
               var rateMapKey = $"rates/{input.JobId}.json";
               await _s3Service.UploadFileAsync(
                   processingBucket,
                   rateMapKey,
                   rateMapPath,
                   context.CancellationToken);
               
               input.RateMapKey = rateMapKey;
               input.Stage = "RATES_FETCHED";
               
               _logger.LogInformation(
                   "Successfully fetched {Count} exchange rates",
                   rateMap.Rates.Count);
               
               // Cleanup
               File.Delete(normalizedPath);
               File.Delete(rateMapPath);
               
               return input;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error fetching NBP rates");
               input.Stage = "RATE_FETCH_ERROR";
               input.ErrorMessage = $"Failed to fetch exchange rates: {ex.Message}";
               throw;
           }
       }
   }
   ```

5. **Configure Lambda Settings**

   Update `aws-lambda-tools-defaults.json`:
   ```json
   {
     "profile": "",
     "region": "eu-central-1",
     "configuration": "Release",
     "function-runtime": "dotnet10",
     "function-memory-size": 512,
     "function-timeout": 600,
     "function-handler": "InvestTax.Lambda.NBPClient::InvestTax.Lambda.NBPClient.Function::FunctionHandler",
     "environment-variables": {
       "PROCESSING_BUCKET": "investtax-processing-dev",
       "NBP_API_URL": "https://api.nbp.pl/api"
     }
   }
   ```

6. **Build and Test**
   ```bash
   cd src/InvestTax.Lambda.NBPClient
   dotnet build
   ```

#### Expected Output
```
Rate Map JSON:
{
  "rates": {
    "USD#2024-03-15": 3.9876,
    "EUR#2024-03-15": 4.3210,
    "USD#2024-06-20": 4.0123,
    "EUR#2024-12-01": 4.3456
  }
}

Lambda Output:
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "stage": "RATES_FETCHED",
  "rateMapKey": "rates/550e8400-e29b-41d4-a716-446655440000.json"
}
```

#### Success Criteria
- [ ] NBP API successfully called with proper endpoint format
- [ ] Polly retry policy handles transient failures
- [ ] Weekend/holiday dates handled (fallback to previous day)
- [ ] Unique currency-date pairs extracted correctly
- [ ] Rate map JSON generated and uploaded to S3
- [ ] All required rates fetched (no missing rates)

#### Troubleshooting
- **404 from NBP API**: Check date format (yyyy-MM-dd) and currency code
- **Rate not found**: Implement previous-day fallback for weekends/holidays
- **Timeout errors**: Increase Lambda timeout or optimize sequential calls
- **Polly not retrying**: Verify policy configuration and exception types

---

### Step 9: Tax Calculator Lambda Implementation

**Duration**: 4-5 hours  
**Dependencies**: Step 8  
**Prerequisites**: NBP rate data available

#### Objective
Implement Lambda function with FIFO matching algorithm to calculate capital gains and tax liability inPLN.

#### Technology Stack
- **.NET Runtime**: 10.0
- **Algorithm**: FIFO (First In, First Out)
- **Decimal Math**: System.Decimal (no floating point)

#### Actions

1. **Create Calculation Models**

   Create `Models/CalculationModels.cs`:
   ```csharp
   namespace InvestTax.Lambda.Calculator.Models;
   
   public class TaxCalculationResult
   {
       public string JobId { get; set; }
       public int Year { get; set; }
       public decimal TotalGainPLN { get; set; }
       public decimal TotalTaxPLN { get; set; }
       public int MatchedTransactionCount { get; set; }
       public List<MatchedTransaction> MatchedTransactions { get; set; } = new();
   }
   
   public class MatchedTransaction
   {
       public int MatchId { get; set; }
       public string ISIN { get; set; }
       public string Ticker { get; set; }
       
       // Buy details
       public DateTime BuyDate { get; set; }
       public decimal BuyShares { get; set; }
       public decimal BuyPricePerShare { get; set; }
       public string BuyCurrency { get; set; }
       public decimal BuyExchangeRate { get; set; }
       public decimal BuyCostPLN { get; set; }
       
       // Sell details
       public DateTime SellDate { get; set; }
       public decimal SellShares { get; set; }
       public decimal SellPricePerShare { get; set; }
       public string SellCurrency { get; set; }
       public decimal SellExchangeRate { get; set; }
       public decimal SellProceedsPLN { get; set; }
       
       // Calculation
       public decimal GainLossPLN { get; set; }
   }
   
   public class FIFOQueue
   {
       public string ISIN { get; set; }
       public Queue<BuyPosition> BuyPositions { get; set; } = new();
   }
   
   public class BuyPosition
   {
       public DateTime Date { get; set; }
       public decimal RemainingShares { get; set; }
       public decimal PricePerShare { get; set; }
       public string Currency { get; set; }
       public decimal ExchangeRate { get; set; }
   }
   ```

2. **Create FIFO Calculator Service**

   Create `Services/TaxCalculatorService.cs`:
   ```csharp
   using InvestTax.Lambda.Calculator.Models;
   using InvestTax.Core.Enums;
   
   namespace InvestTax.Lambda.Calculator.Services;
   
   public class TaxCalculatorService
   {
       private readonly ILogger<TaxCalculatorService> _logger;
       private const decimal TAX_RATE = 0.19m; // 19% Polish capital gains tax
       private const int DECIMAL_PLACES = 2;
       
       public TaxCalculatorService(ILogger<TaxCalculatorService> logger)
       {
           _logger = logger;
       }
       
       public TaxCalculationResult CalculateTax(
           Dictionary<string, TransactionGroup> transactionGroups,
           Dictionary<string, decimal> rateMap,
           int year)
       {
           var result = new TaxCalculationResult
           {
               Year = year
           };
           
           var matchId = 1;
           
           // Process each ISIN group separately
           foreach (var group in transactionGroups.Values)
           {
               _logger.LogInformation(
                   "Processing ISIN: {ISIN} with {Count} transactions",
                   group.ISIN, group.Transactions.Count);
               
               var fifoQueue = new FIFOQueue { ISIN = group.ISIN };
               
               foreach (var transaction in group.Transactions.OrderBy(t => t.TransactionDate))
               {
                   if (transaction.Action == TransactionAction.Buy)
                   {
                       ProcessBuy(fifoQueue, transaction, rateMap);
                   }
                   else if (transaction.Action == TransactionAction.Sell)
                   {
                       var matches = ProcessSell(
                           fifoQueue,
                           transaction,
                           rateMap,
                           matchId);
                       
                       result.MatchedTransactions.AddRange(matches);
                       matchId += matches.Count;
                   }
               }
               
               // Warn if there are remaining buy positions (not sold)
               if (fifoQueue.BuyPositions.Count > 0)
               {
                   var totalRemaining = fifoQueue.BuyPositions.Sum(p => p.RemainingShares);
                   _logger.LogInformation(
                       "ISIN {ISIN} has {Shares} shares remaining (not sold)",
                       group.ISIN, totalRemaining);
               }
           }
           
           // Calculate totals
           result.TotalGainPLN = result.MatchedTransactions.Sum(m => m.GainLossPLN);
           result.TotalTaxPLN = Math.Max(0, Math.Round(result.TotalGainPLN * TAX_RATE, DECIMAL_PLACES));
           result.MatchedTransactionCount = result.MatchedTransactions.Count;
           
           _logger.LogInformation(
               "Tax calculation complete: Gain={Gain} PLN, Tax={Tax} PLN, Matches={Count}",
               result.TotalGainPLN, result.TotalTaxPLN, result.MatchedTransactionCount);
           
           return result;
       }
       
       private void ProcessBuy(
           FIFOQueue queue,
           NormalizedTransaction transaction,
           Dictionary<string, decimal> rateMap)
       {
           var exchangeRate = GetExchangeRate(
               transaction.Currency,
               transaction.TransactionDate,
               rateMap);
           
           queue.BuyPositions.Enqueue(new BuyPosition
           {
               Date = transaction.TransactionDate,
               RemainingShares = transaction.Shares,
               PricePerShare = transaction.PricePerShare,
               Currency = transaction.Currency,
               ExchangeRate = exchangeRate
           });
           
           _logger.LogDebug(
               "Buy: {Shares} shares of {ISIN} at {Price} {Currency} (Rate: {Rate})",
               transaction.Shares, queue.ISIN, transaction.PricePerShare,
               transaction.Currency, exchangeRate);
       }
       
       private List<MatchedTransaction> ProcessSell(
           FIFOQueue queue,
           NormalizedTransaction sellTransaction,
           Dictionary<string, decimal> rateMap,
           int startMatchId)
       {
           var matches = new List<MatchedTransaction>();
           var remainingSharesToSell = sellTransaction.Shares;
           var matchId = startMatchId;
           
           var sellExchangeRate = GetExchangeRate(
               sellTransaction.Currency,
               sellTransaction.TransactionDate,
               rateMap);
           
           while (remainingSharesToSell > 0 && queue.BuyPositions.Count > 0)
           {
               var buyPosition = queue.BuyPositions.Peek();
               
               // Determine how many shares to match
               var sharesToMatch = Math.Min(remainingSharesToSell, buyPosition.RemainingShares);
               
               // Calculate PLN values
               var buyCostPLN = Math.Round(
                   sharesToMatch * buyPosition.PricePerShare * buyPosition.ExchangeRate,
                   DECIMAL_PLACES);
               
               var sellProceedsPLN = Math.Round(
                   sharesToMatch * sellTransaction.PricePerShare * sellExchangeRate,
                   DECIMAL_PLACES);
               
               var gainLossPLN = Math.Round(sellProceedsPLN - buyCostPLN, DECIMAL_PLACES);
               
               // Create matched transaction record
               matches.Add(new MatchedTransaction
               {
                   MatchId = matchId++,
                   ISIN = queue.ISIN,
                   Ticker = sellTransaction.Ticker,
                   BuyDate = buyPosition.Date,
                   BuyShares = sharesToMatch,
                   BuyPricePerShare = buyPosition.PricePerShare,
                   BuyCurrency = buyPosition.Currency,
                   BuyExchangeRate = buyPosition.ExchangeRate,
                   BuyCostPLN = buyCostPLN,
                   SellDate = sellTransaction.TransactionDate,
                   SellShares = sharesToMatch,
                   SellPricePerShare = sellTransaction.PricePerShare,
                   SellCurrency = sellTransaction.Currency,
                   SellExchangeRate = sellExchangeRate,
                   SellProceedsPLN = sellProceedsPLN,
                   GainLossPLN = gainLossPLN
               });
               
               _logger.LogDebug(
                   "Match: Sell {Shares} shares - Cost: {Cost} PLN, Proceeds: {Proceeds} PLN, Gain: {Gain} PLN",
                   sharesToMatch, buyCostPLN, sellProceedsPLN, gainLossPLN);
               
               // Update remaining shares
               buyPosition.RemainingShares -= sharesToMatch;
               remainingSharesToSell -= sharesToMatch;
               
               // Remove buy position if fully consumed
               if (buyPosition.RemainingShares == 0)
               {
                   queue.BuyPositions.Dequeue();
               }
           }
           
           if (remainingSharesToSell > 0)
           {
               throw new InvalidOperationException(
                   $"Attempted to sell {remainingSharesToSell} more shares than available " +
                   $"for {queue.ISIN} on {sellTransaction.TransactionDate:yyyy-MM-dd}");
           }
           
           return matches;
       }
       
       private decimal GetExchangeRate(
           string currency,
           DateTime date,
           Dictionary<string, decimal> rateMap)
       {
           // PLN transactions have rate of 1.0
           if (currency == "PLN")
               return 1.0m;
           
           var key = $"{currency}#{date:yyyy-MM-dd}";
           
           if (!rateMap.TryGetValue(key, out var rate))
           {
               throw new InvalidOperationException(
                   $"Exchange rate not found for {currency} on {date:yyyy-MM-dd}");
           }
           
           return rate;
       }
   }
   ```

3. **Implement Lambda Handler**

   Update `Function.cs`:
   ```csharp
   using Amazon.Lambda.Core;
   using Amazon.S3;
   using InvestTax.Core.Models;
   using InvestTax.Infrastructure.AWS;
   using InvestTax.Lambda.Calculator.Services;
   using InvestTax.Lambda.Calculator.Models;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   using System.Text.Json;
   
   [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
   
   namespace InvestTax.Lambda.Calculator;
   
   public class Function
   {
       private readonly IS3Service _s3Service;
       private readonly TaxCalculatorService _calculatorService;
       private readonly ILogger<Function> _logger;
       
       public Function()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           var serviceProvider = services.BuildServiceProvider();
           
           _s3Service = serviceProvider.GetRequiredService<IS3Service>();
           _calculatorService = serviceProvider.GetRequiredService<TaxCalculatorService>();
           _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
       }
       
       private void ConfigureServices(IServiceCollection services)
       {
           services.AddLogging(builder =>
           {
               builder.AddConsole();
               builder.AddLambdaLogger();
           });
           
           services.AddAWSService<IAmazonS3>();
           services.AddSingleton<IS3Service, S3Service>();
           services.AddSingleton<TaxCalculatorService>();
       }
       
       public async Task<LambdaInput> FunctionHandler(
           LambdaInput input,
           ILambdaContext context)
       {
           _logger.LogInformation("Starting tax calculation for JobId: {JobId}", input.JobId);
           
           try
           {
               var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET");
               
               // Download normalized transactions
               var normalizedPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_normalized.json");
               await _s3Service.DownloadFileAsync(
                   processingBucket,
                   input.NormalizedFileKey,
                   normalizedPath,
                   context.CancellationToken);
               
               var transactionsJson = await File.ReadAllTextAsync(normalizedPath, context.CancellationToken);
               var transactionGroups = JsonSerializer.Deserialize<Dictionary<string, TransactionGroup>>(transactionsJson);
               
               // Download rate map
               var rateMapPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_rates.json");
               await _s3Service.DownloadFileAsync(
                   processingBucket,
                   input.RateMapKey,
                   rateMapPath,
                   context.CancellationToken);
               
               var rateMapJson = await File.ReadAllTextAsync(rateMapPath, context.CancellationToken);
               var rateMap = JsonSerializer.Deserialize<RateMap>(rateMapJson);
               
               // Calculate tax
               var calculationResult = _calculatorService.CalculateTax(
                   transactionGroups,
                   rateMap.Rates,
                   input.Year);
               
               calculationResult.JobId = input.JobId;
               
               // Save calculation results
               var resultPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_calculation.json");
               var resultJson = JsonSerializer.Serialize(calculationResult, new JsonSerializerOptions
               {
                   WriteIndented = true
               });
               await File.WriteAllTextAsync(resultPath, resultJson, context.CancellationToken);
               
               var resultKey = $"calculations/{input.JobId}.json";
               await _s3Service.UploadFileAsync(
                   processingBucket,
                   resultKey,
                   resultPath,
                   context.CancellationToken);
               
               input.CalculationResultKey = resultKey;
               input.TotalGainPLN = calculationResult.TotalGainPLN;
               input.TotalTaxPLN = calculationResult.TotalTaxPLN;
               input.Stage = "CALCULATED";
               
               _logger.LogInformation(
                   "Tax calculation complete: Gain={Gain} PLN, Tax={Tax} PLN",
                   calculationResult.TotalGainPLN, calculationResult.TotalTaxPLN);
               
               // Cleanup
               File.Delete(normalizedPath);
               File.Delete(rateMapPath);
               File.Delete(resultPath);
               
               return input;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error calculating tax");
               input.Stage = "CALCULATION_ERROR";
               input.ErrorMessage = $"Tax calculation failed: {ex.Message}";
               throw;
           }
       }
   }
   ```

4. **Configure Lambda Settings**

   Update `aws-lambda-tools-defaults.json`:
   ```json
   {
     "profile": "",
     "region": "eu-central-1",
     "configuration": "Release",
     "function-runtime": "dotnet10",
     "function-memory-size": 2048,
     "function-timeout": 600,
     "function-handler": "InvestTax.Lambda.Calculator::InvestTax.Lambda.Calculator.Function::FunctionHandler",
     "environment-variables": {
       "PROCESSING_BUCKET": "investtax-processing-dev"
     }
   }
   ```

5. **Build and Test**
   ```bash
   cd src/InvestTax.Lambda.Calculator
   dotnet build
   ```

#### Expected Output
```
Calculation Result JSON:
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "year": 2024,
  "totalGainPLN": 15000.50,
  "totalTaxPLN": 2850.09,
  "matchedTransactionCount": 45,
  "matchedTransactions": [
    {
      "matchId": 1,
      "isin": "US0378331005",
      "ticker": "AAPL",
      "buyDate": "2024-03-15",
      "buyShares": 10,
      "buyPricePerShare": 170.50,
      "buyCurrency": "USD",
      "buyExchangeRate": 3.9876,
      "buyCostPLN": 6797.64,
      "sellDate": "2024-06-20",
      "sellShares": 10,
      "sellPricePerShare": 185.00,
      "sellCurrency": "USD",
      "sellExchangeRate": 4.0123,
      "sellProceedsPLN": 7422.76,
      "gainLossPLN": 625.12
    }
  ]
}
```

#### Success Criteria
- [ ] FIFO algorithm correctly matches buys to sells
- [ ] Partial fills handled correctly
- [ ] PLN conversions accurate with proper rounding (2 decimals)
- [ ] Tax calculated at 19% of total gains
- [ ] All matched transactions recorded with details
- [ ] Remaining (unsold) positions logged
- [ ] Error thrown if attempting to sell more than available

#### Troubleshooting
- **FIFO matching errors**: Verify transactions sorted by date
- **Decimal precision issues**: Ensure using decimal type, not float/double
- **Missing exchange rates**: Verify rate map contains all required dates
- **Sell exceeds buy**: Check validation logic in earlier stages

---

### Step 10: Report Generator Lambda Implementation

**Duration**: 3-4 hours  
**Dependencies**: Step 9  
**Prerequisites**: Tax calculation results available

#### Objective
Implement Lambda function to generate human-readable plain text report summarizing tax calculation results (MVP: text only, Phase 2 adds HTML).

#### Technology Stack
- **.NET Runtime**: 10.0
- **Template Engine**: StringBuilder (simple text formatting)
- **Formatting**: String interpolation and formatting

#### Actions

1. **Create Report Models**

   Create `Models/ReportModels.cs`:
   ```csharp
   namespace InvestTax.Lambda.ReportGenerator.Models;
   
   public class TaxReport
   {
       public string JobId { get; set; }
       public int Year { get; set; }
       public DateTime GeneratedDate { get; set; }
       public string TextReportKey { get; set; }
       
       // Summary data
       public decimal TotalGainPLN { get; set; }
       public decimal TotalTaxPLN { get; set; }
       public int TransactionCount { get; set; }
       
       // Detailed transactions
       public List<MatchedTransaction> Transactions { get; set; } = new();
   }
   ```

2. **Create Report Generator Service**

   Create `Services/ReportGeneratorService.cs`:
   ```csharp
   using InvestTax.Lambda.ReportGenerator.Models;
   using InvestTax.Lambda.Calculator.Models;
   using System.Text;
   
   namespace InvestTax.Lambda.ReportGenerator.Services;
   
   public class ReportGeneratorService
   {
       private readonly ILogger<ReportGeneratorService> _logger;
       
       public ReportGeneratorService(ILogger<ReportGeneratorService> logger)
       {
           _logger = logger;
       }
       
       public string GenerateTextReport(TaxCalculationResult calculation)
       {
           var sb = new StringBuilder();
           
           // Header
           sb.AppendLine("═══════════════════════════════════════════════════════════════");
           sb.AppendLine("           POLISH CAPITAL GAINS TAX CALCULATION (PIT-38)       ");
           sb.AppendLine("═══════════════════════════════════════════════════════════════");
           sb.AppendLine();
           sb.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
           sb.AppendLine($"Job ID: {calculation.JobId}");
           sb.AppendLine($"Tax Year: {calculation.Year}");
           sb.AppendLine();
           
           // Summary Section
           sb.AppendLine("───────────────────────────────────────────────────────────────");
           sb.AppendLine("  SUMMARY");
           sb.AppendLine("───────────────────────────────────────────────────────────────");
           sb.AppendLine();
           sb.AppendLine($"  Total Capital Gain:     {calculation.TotalGainPLN,15:N2} PLN");
           sb.AppendLine($"  Tax Rate:               {19,15:N0}%");
           sb.AppendLine($"  Total Tax Owed:         {calculation.TotalTaxPLN,15:N2} PLN");
           sb.AppendLine();
           sb.AppendLine($"  Matched Transactions:   {calculation.MatchedTransactionCount,15:N0}");
           sb.AppendLine();
           
           // Methodology Section
           sb.AppendLine("───────────────────────────────────────────────────────────────");
           sb.AppendLine("  METHODOLOGY");
           sb.AppendLine("───────────────────────────────────────────────────────────────");
           sb.AppendLine();
           sb.AppendLine("  • FIFO Matching: First-In, First-Out method for buy-sell matching");
           sb.AppendLine("  • Exchange Rates: National Bank of Poland (NBP) official rates");
           sb.AppendLine("  • Tax Rate: 19% flat rate on capital gains (Polish tax law)");
           sb.AppendLine("  • Rounding: All PLN amounts rounded to 2 decimal places");
           sb.AppendLine();
           
           // Detailed Transactions
           sb.AppendLine("───────────────────────────────────────────────────────────────");
           sb.AppendLine("  DETAILED TRANSACTIONS");
           sb.AppendLine("───────────────────────────────────────────────────────────────");
           sb.AppendLine();
           
           // Group by ISIN
           var groupedByISIN = calculation.MatchedTransactions
               .GroupBy(t => t.ISIN)
               .OrderBy(g => g.Key);
           
           foreach (var isinGroup in groupedByISIN)
           {
               var isin = isinGroup.Key;
               var ticker = isinGroup.First().Ticker;
               var totalGainForISIN = isinGroup.Sum(t => t.GainLossPLN);
               
               sb.AppendLine($"▸ {ticker} ({isin})");
               sb.AppendLine($"  Total Gain/Loss: {totalGainForISIN:N2} PLN");
               sb.AppendLine();
               
               foreach (var match in isinGroup.OrderBy(t => t.SellDate))
               {
                   sb.AppendLine($"  Match #{match.MatchId}:");
                   sb.AppendLine($"    BUY:  {match.BuyDate:yyyy-MM-dd}");
                   sb.AppendLine($"          {match.BuyShares:N4} shares @ {match.BuyPricePerShare:N2} {match.BuyCurrency}");
                   sb.AppendLine($"          Exchange Rate: {match.BuyExchangeRate:N4} PLN/{match.BuyCurrency}");
                   sb.AppendLine($"          Cost: {match.BuyCostPLN:N2} PLN");
                   sb.AppendLine();
                   sb.AppendLine($"    SELL: {match.SellDate:yyyy-MM-dd}");
                   sb.AppendLine($"          {match.SellShares:N4} shares @ {match.SellPricePerShare:N2} {match.SellCurrency}");
                   sb.AppendLine($"          Exchange Rate: {match.SellExchangeRate:N4} PLN/{match.SellCurrency}");
                   sb.AppendLine($"          Proceeds: {match.SellProceedsPLN:N2} PLN");
                   sb.AppendLine();
                   
                   var gainLabel = match.GainLossPLN >= 0 ? "GAIN" : "LOSS";
                   sb.AppendLine($"    {gainLabel}: {match.GainLossPLN:N2} PLN");
                   sb.AppendLine();
                   sb.AppendLine("  " + new string('-', 60));
                   sb.AppendLine();
               }
           }
           
           // Disclaimer
           sb.AppendLine();
           sb.AppendLine("═══════════════════════════════════════════════════════════════");
           sb.AppendLine("  IMPORTANT DISCLAIMER");
           sb.AppendLine("═══════════════════════════════════════════════════════════════");
           sb.AppendLine();
           sb.AppendLine("This calculation is provided for INFORMATIONAL PURPOSES ONLY.");
           sb.AppendLine();
           sb.AppendLine("• This is NOT official tax advice");
           sb.AppendLine("• You are responsible for verifying all calculations");
           sb.AppendLine("• Polish tax law is subject to change");
           sb.AppendLine("• Corporate actions (splits, dividends) must be handled separately");
           sb.AppendLine("• Consult a qualified tax professional before filing");
           sb.AppendLine();
           sb.AppendLine("Data Sources:");
           sb.AppendLine("• Exchange Rates: National Bank of Poland (NBP) official rates");
           sb.AppendLine("• Methodology: FIFO (First-In, First-Out) matching");
           sb.AppendLine();
           sb.AppendLine("═══════════════════════════════════════════════════════════════");
           sb.AppendLine("              END OF REPORT");
           sb.AppendLine("═══════════════════════════════════════════════════════════════");
           
           var report = sb.ToString();
           
           _logger.LogInformation(
               "Generated text report: {Length} characters",
               report.Length);
           
           return report;
       }
   }
   ```

3. **Implement Lambda Handler**

   Update `Function.cs`:
   ```csharp
   using Amazon.Lambda.Core;
   using Amazon.S3;
   using InvestTax.Core.Models;
   using InvestTax.Infrastructure.AWS;
   using InvestTax.Lambda.ReportGenerator.Services;
   using InvestTax.Lambda.Calculator.Models;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   using System.Text;
   using System.Text.Json;
   
   [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
   
   namespace InvestTax.Lambda.ReportGenerator;
   
   public class Function
   {
       private readonly IS3Service _s3Service;
       private readonly ReportGeneratorService _reportGenerator;
       private readonly ILogger<Function> _logger;
       
       public Function()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           var serviceProvider = services.BuildServiceProvider();
           
           _s3Service = serviceProvider.GetRequiredService<IS3Service>();
           _reportGenerator = serviceProvider.GetRequiredService<ReportGeneratorService>();
           _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
       }
       
       private void ConfigureServices(IServiceCollection services)
       {
           services.AddLogging(builder =>
           {
               builder.AddConsole();
               builder.AddLambdaLogger();
           });
           
           services.AddAWSService<IAmazonS3>();
           services.AddSingleton<IS3Service, S3Service>();
           services.AddSingleton<ReportGeneratorService>();
       }
       
       public async Task<LambdaInput> FunctionHandler(
           LambdaInput input,
           ILambdaContext context)
       {
           _logger.LogInformation("Starting report generation for JobId: {JobId}", input.JobId);
           
           try
           {
               var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET");
               
               // Download calculation results
               var calculationPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_calculation.json");
               await _s3Service.DownloadFileAsync(
                   processingBucket,
                   input.CalculationResultKey,
                   calculationPath,
                   context.CancellationToken);
               
               var calculationJson = await File.ReadAllTextAsync(calculationPath, context.CancellationToken);
               var calculation = JsonSerializer.Deserialize<TaxCalculationResult>(calculationJson);
               
               // Generate text report
               var textReport = _reportGenerator.GenerateTextReport(calculation);
               
               // Save text report
               var textReportPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_report.txt");
               await File.WriteAllTextAsync(textReportPath, textReport, Encoding.UTF8, context.CancellationToken);
               
               var textReportKey = $"reports/{input.JobId}.txt";
               await _s3Service.UploadFileAsync(
                   processingBucket,
                   textReportKey,
                   textReportPath,
                   context.CancellationToken);
               
               input.TextReportKey = textReportKey;
               input.Stage = "REPORT_GENERATED";
               
               _logger.LogInformation("Report generation complete");
               
               // Cleanup
               File.Delete(calculationPath);
               File.Delete(textReportPath);
               
               return input;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error generating report");
               input.Stage = "REPORT_ERROR";
               input.ErrorMessage = $"Report generation failed: {ex.Message}";
               throw;
           }
       }
   }
   ```

4. **Configure Lambda Settings**

   Update `aws-lambda-tools-defaults.json`:
   ```json
   {
     "profile": "",
     "region": "eu-central-1",
     "configuration": "Release",
     "function-runtime": "dotnet10",
     "function-memory-size": 512,
     "function-timeout": 180,
     "function-handler": "InvestTax.Lambda.ReportGenerator::InvestTax.Lambda.ReportGenerator.Function::FunctionHandler",
     "environment-variables": {
       "PROCESSING_BUCKET": "investtax-processing-dev"
     }
   }
   ```

5. **Build and Test**
   ```bash
   cd src/InvestTax.Lambda.ReportGenerator
   dotnet build
   ```

#### Expected Output
```
Text Report Sample:
═══════════════════════════════════════════════════════════════
           POLISH CAPITAL GAINS TAX CALCULATION (PIT-38)       
═══════════════════════════════════════════════════════════════

Report Generated: 2024-06-20 15:30:00 UTC
Job ID: 550e8400-e29b-41d4-a716-446655440000
Tax Year: 2024

───────────────────────────────────────────────────────────────
  SUMMARY
───────────────────────────────────────────────────────────────

  Total Capital Gain:        15,000.50 PLN
  Tax Rate:                          19%
  Total Tax Owed:             2,850.09 PLN

  Matched Transactions:              45

[... detailed transactions ...]

Lambda Output:
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "stage": "REPORT_GENERATED",
  "textReportKey": "reports/550e8400-e29b-41d4-a716-446655440000.txt"
}
```

#### Success Criteria
- [ ] Text report generated with proper formatting
- [ ] Summary section displays key metrics
- [ ] Detailed transactions grouped by ISIN
- [ ] Methodology and disclaimer included
- [ ] UTF-8 encoding preserved
- [ ] Report uploaded to S3 successfully

#### Troubleshooting
- **Formatting issues**: Check StringBuilder line breaks and spacing
- **Encoding errors**: Ensure UTF-8 encoding when writing files
- **Missing data**: Verify calculation JSON deserialization
- **Large reports**: Increase timeout for files with many transactions

---

### Step 11: Email Sender Lambda Implementation

**Duration**: 3-4 hours  
**Dependencies**: Step 10  
**Prerequisites**: SES configured and verified

#### Objective
Implement Lambda function to send tax calculation reports via Amazon SES with both success and error email templates.

#### Technology Stack
- **.NET Runtime**: 10.0
- **Email Service**: Amazon SES via AWSSDK.SimpleEmail
- **Templates**: String-based email templates

#### Actions

1. **Add NuGet Packages**
   ```bash
   cd src/InvestTax.Lambda.EmailSender
   dotnet add package AWSSDK.SimpleEmail --version 3.7.400
   ```

2. **Create Email Models**

   Create `Models/EmailModels.cs`:
   ```csharp
   namespace InvestTax.Lambda.EmailSender.Models;
   
   public class EmailRequest
   {
       public string ToAddress { get; set; }
       public string Subject { get; set; }
       public string TextBody { get; set; }
       public string HtmlBody { get; set; }
       public List<Attachment> Attachments { get; set; } = new();
   }
   
   public class Attachment
   {
       public string FileName { get; set; }
       public byte[] Content { get; set; }
       public string ContentType { get; set; }
   }
   
   public class EmailResult
   {
       public bool Success { get; set; }
       public string MessageId { get; set; }
       public string ErrorMessage { get; set; }
   }
   ```

3. **Create Email Service**

   Create `Services/EmailService.cs`:
   ```csharp
   using Amazon.SimpleEmail;
   using Amazon.SimpleEmail.Model;
   using InvestTax.Lambda.EmailSender.Models;
   using System.Text;
   
   namespace InvestTax.Lambda.EmailSender.Services;
   
   public class EmailService
   {
       private readonly IAmazonSimpleEmailService _sesClient;
       private readonly ILogger<EmailService> _logger;
       private readonly string _fromAddress;
       
       public EmailService(
           IAmazonSimpleEmailService sesClient,
           ILogger<EmailService> logger)
       {
           _sesClient = sesClient;
           _logger = logger;
           _fromAddress = Environment.GetEnvironmentVariable("SES_FROM_EMAIL") 
               ?? "noreply@investtax.example.com";
       }
       
       public async Task<EmailResult> SendSuccessEmailAsync(
           string toAddress,
           string reportContent,
           string jobId,
           decimal totalGainPLN,
           decimal totalTaxPLN,
           int year,
           CancellationToken cancellationToken)
       {
           var subject = $"InvestTax Calculator - Tax Report {year} (Job: {jobId})";
           
           var textBody = BuildSuccessEmailText(
               reportContent,
               jobId,
               totalGainPLN,
               totalTaxPLN,
               year);
           
           var htmlBody = BuildSuccessEmailHtml(
               jobId,
               totalGainPLN,
               totalTaxPLN,
               year);
           
           return await SendEmailAsync(
               toAddress,
               subject,
               textBody,
               htmlBody,
               cancellationToken);
       }
       
       public async Task<EmailResult> SendErrorEmailAsync(
           string toAddress,
           string jobId,
           string stage,
           string errorMessage,
           CancellationToken cancellationToken)
       {
           var subject = $"InvestTax Calculator - Processing Error (Job: {jobId})";
           
           var textBody = BuildErrorEmailText(jobId, stage, errorMessage);
           var htmlBody = BuildErrorEmailHtml(jobId, stage, errorMessage);
           
           return await SendEmailAsync(
               toAddress,
               subject,
               textBody,
               htmlBody,
               cancellationToken);
       }
       
       private async Task<EmailResult> SendEmailAsync(
           string toAddress,
           string subject,
           string textBody,
           string htmlBody,
           CancellationToken cancellationToken)
       {
           try
           {
               var request = new SendEmailRequest
               {
                   Source = _fromAddress,
                   Destination = new Destination
                   {
                       ToAddresses = new List<string> { toAddress }
                   },
                   Message = new Amazon.SimpleEmail.Model.Message
                   {
                       Subject = new Content(subject),
                       Body = new Body
                       {
                           Text = new Content(textBody),
                           Html = new Content(htmlBody)
                       }
                   }
               };
               
               _logger.LogInformation(
                   "Sending email to {ToAddress} with subject: {Subject}",
                   toAddress, subject);
               
               var response = await _sesClient.SendEmailAsync(request, cancellationToken);
               
               _logger.LogInformation(
                   "Email sent successfully. MessageId: {MessageId}",
                   response.MessageId);
               
               return new EmailResult
               {
                   Success = true,
                   MessageId = response.MessageId
               };
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Failed to send email to {ToAddress}", toAddress);
               
               return new EmailResult
               {
                   Success = false,
                   ErrorMessage = ex.Message
               };
           }
       }
       
       private string BuildSuccessEmailText(
           string reportContent,
           string jobId,
           decimal totalGainPLN,
           decimal totalTaxPLN,
           int year)
       {
           var sb = new StringBuilder();
           
           sb.AppendLine("InvestTax Calculator - Tax Calculation Complete");
           sb.AppendLine();
           sb.AppendLine($"Your tax calculation for year {year} has been completed successfully.");
           sb.AppendLine();
           sb.AppendLine("SUMMARY:");
           sb.AppendLine($"  Job ID: {jobId}");
           sb.AppendLine($"  Tax Year: {year}");
           sb.AppendLine($"  Total Capital Gain: {totalGainPLN:N2} PLN");
           sb.AppendLine($"  Total Tax Owed (19%): {totalTaxPLN:N2} PLN");
           sb.AppendLine();
           sb.AppendLine("DETAILED REPORT:");
           sb.AppendLine(new string('=', 70));
           sb.AppendLine();
           sb.AppendLine(reportContent);
           sb.AppendLine();
           sb.AppendLine(new string('=', 70));
           sb.AppendLine();
           sb.AppendLine("IMPORTANT: This calculation is for informational purposes only.");
           sb.AppendLine("Please verify all amounts and consult a tax professional before filing.");
           sb.AppendLine();
           sb.AppendLine("Thank you for using InvestTax Calculator!");
           
           return sb.ToString();
       }
       
       private string BuildSuccessEmailHtml(
           string jobId,
           decimal totalGainPLN,
           decimal totalTaxPLN,
           int year)
       {
           return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .summary {{ background-color: white; padding: 15px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .disclaimer {{ background-color: #fff3cd; padding: 15px; margin: 20px 0; border-left: 4px solid #ffc107; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✓ Tax Calculation Complete</h1>
        </div>
        <div class='content'>
            <p>Your tax calculation for year <strong>{year}</strong> has been completed successfully.</p>
            
            <div class='summary'>
                <h2>Summary</h2>
                <p><strong>Job ID:</strong> {jobId}</p>
                <p><strong>Tax Year:</strong> {year}</p>
                <p><strong>Total Capital Gain:</strong> {totalGainPLN:N2} PLN</p>
                <p><strong>Total Tax Owed (19%):</strong> {totalTaxPLN:N2} PLN</p>
            </div>
            
            <p>The detailed report is included in the plain text version of this email (view below).</p>
            
            <div class='disclaimer'>
                <h3>⚠ Important Disclaimer</h3>
                <p>This calculation is for <strong>informational purposes only</strong>.</p>
                <p>• This is not official tax advice</p>
                <p>• Verify all amounts before filing</p>
                <p>• Consult a qualified tax professional</p>
            </div>
        </div>
        <div class='footer'>
            <p>Thank you for using InvestTax Calculator</p>
            <p>© 2024 InvestTax Calculator. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
       }
       
       private string BuildErrorEmailText(
           string jobId,
           string stage,
           string errorMessage)
       {
           var sb = new StringBuilder();
           
           sb.AppendLine("InvestTax Calculator - Processing Error");
           sb.AppendLine();
           sb.AppendLine("Unfortunately, your tax calculation could not be completed.");
           sb.AppendLine();
           sb.AppendLine("ERROR DETAILS:");
           sb.AppendLine($"  Job ID: {jobId}");
           sb.AppendLine($"  Stage: {stage}");
           sb.AppendLine($"  Error: {errorMessage}");
           sb.AppendLine();
           sb.AppendLine("WHAT TO DO NEXT:");
           sb.AppendLine();
           
           if (stage.Contains("VALIDATION"))
           {
               sb.AppendLine("• Review the error message above for specific validation issues");
               sb.AppendLine("• Check that your CSV file uses pipe (|) delimiters");
               sb.AppendLine("• Ensure all required columns are present with correct headers");
               sb.AppendLine("• Verify that dates are in valid format (YYYY-MM-DD or ISO 8601)");
               sb.AppendLine("• Check that numeric values are positive and properly formatted");
               sb.AppendLine("• Correct the issues and re-upload your file");
           }
           else if (stage.Contains("RATE"))
           {
               sb.AppendLine("• The system could not fetch exchange rates from NBP API");
               sb.AppendLine("• This may be a temporary issue with the external API");
               sb.AppendLine("• Try uploading your file again in a few minutes");
               sb.AppendLine("• If the problem persists, contact support");
           }
           else
           {
               sb.AppendLine("• An unexpected error occurred during processing");
               sb.AppendLine("• Please contact support with the Job ID above");
               sb.AppendLine("• We apologize for the inconvenience");
           }
           
           sb.AppendLine();
           sb.AppendLine("If you need assistance, please contact support@investtax.example.com");
           sb.AppendLine($"Include Job ID: {jobId}");
           
           return sb.ToString();
       }
       
       private string BuildErrorEmailHtml(
           string jobId,
           string stage,
           string errorMessage)
       {
           return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .error-box {{ background-color: #f8d7da; padding: 15px; margin: 20px 0; border-left: 4px solid #dc3545; }}
        .action-box {{ background-color: white; padding: 15px; margin: 20px 0; border-left: 4px solid #007bff; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✗ Processing Error</h1>
        </div>
        <div class='content'>
            <p>Unfortunately, your tax calculation could not be completed.</p>
            
            <div class='error-box'>
                <h2>Error Details</h2>
                <p><strong>Job ID:</strong> {jobId}</p>
                <p><strong>Stage:</strong> {stage}</p>
                <p><strong>Error:</strong> {errorMessage}</p>
            </div>
            
            <div class='action-box'>
                <h2>What to Do Next</h2>
                <ul>
                    <li>Review the error message above</li>
                    <li>Correct any issues in your CSV file</li>
                    <li>Re-upload your file to try again</li>
                </ul>
            </div>
            
            <p>If you need assistance, please contact <a href='mailto:support@investtax.example.com'>support@investtax.example.com</a> and include Job ID: <strong>{jobId}</strong></p>
        </div>
        <div class='footer'>
            <p>InvestTax Calculator</p>
        </div>
    </div>
</body>
</html>";
       }
   }
   ```

4. **Implement Lambda Handler**

   Update `Function.cs`:
   ```csharp
   using Amazon.Lambda.Core;
   using Amazon.S3;
   using Amazon.SimpleEmail;
   using InvestTax.Core.Models;
   using InvestTax.Infrastructure.AWS;
   using InvestTax.Lambda.EmailSender.Services;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   
   [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
   
   namespace InvestTax.Lambda.EmailSender;
   
   public class Function
   {
       private readonly IS3Service _s3Service;
       private readonly EmailService _emailService;
       private readonly ILogger<Function> _logger;
       
       public Function()
       {
           var services = new ServiceCollection();
           ConfigureServices(services);
           var serviceProvider = services.BuildServiceProvider();
           
           _s3Service = serviceProvider.GetRequiredService<IS3Service>();
           _emailService = serviceProvider.GetRequiredService<EmailService>();
           _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
       }
       
       private void ConfigureServices(IServiceCollection services)
       {
           services.AddLogging(builder =>
           {
               builder.AddConsole();
               builder.AddLambdaLogger();
           });
           
           services.AddAWSService<IAmazonS3>();
           services.AddAWSService<IAmazonSimpleEmailService>();
           services.AddSingleton<IS3Service, S3Service>();
           services.AddSingleton<EmailService>();
       }
       
       public async Task<LambdaInput> FunctionHandler(
           LambdaInput input,
           ILambdaContext context)
       {
           _logger.LogInformation("Starting email send for JobId: {JobId}", input.JobId);
           
           try
           {
               var processingBucket = Environment.GetEnvironmentVariable("PROCESSING_BUCKET");
               EmailResult result;
               
               if (input.Stage == "REPORT_GENERATED")
               {
                   // Success email with report
                   var reportPath = Path.Combine(Path.GetTempPath(), $"{input.JobId}_report.txt");
                   await _s3Service.DownloadFileAsync(
                       processingBucket,
                       input.TextReportKey,
                       reportPath,
                       context.CancellationToken);
                   
                   var reportContent = await File.ReadAllTextAsync(reportPath, context.CancellationToken);
                   
                   result = await _emailService.SendSuccessEmailAsync(
                       input.Email,
                       reportContent,
                       input.JobId,
                       input.TotalGainPLN,
                       input.TotalTaxPLN,
                       input.Year,
                       context.CancellationToken);
                   
                   File.Delete(reportPath);
               }
               else
               {
                   // Error email
                   result = await _emailService.SendErrorEmailAsync(
                       input.Email,
                       input.JobId,
                       input.Stage,
                       input.ErrorMessage ?? "Unknown error",
                       context.CancellationToken);
               }
               
               if (result.Success)
               {
                   input.Stage = "EMAIL_SENT";
                   input.EmailMessageId = result.MessageId;
                   _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", result.MessageId);
               }
               else
               {
                   input.Stage = "EMAIL_FAILED";
                   input.ErrorMessage = result.ErrorMessage;
                   _logger.LogError("Email sending failed: {Error}", result.ErrorMessage);
               }
               
               return input;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error in email sender");
               input.Stage = "EMAIL_ERROR";
               input.ErrorMessage = ex.Message;
               throw;
           }
       }
   }
   ```

5. **Configure Lambda Settings**

   Update `aws-lambda-tools-defaults.json`:
   ```json
   {
     "profile": "",
     "region": "eu-central-1",
     "configuration": "Release",
     "function-runtime": "dotnet10",
     "function-memory-size": 256,
     "function-timeout": 60,
     "function-handler": "InvestTax.Lambda.EmailSender::InvestTax.Lambda.EmailSender.Function::FunctionHandler",
     "environment-variables": {
       "PROCESSING_BUCKET": "investtax-processing-dev",
       "SES_FROM_EMAIL": "noreply@investtax.example.com"
     }
   }
   ```

6. **Build and Test**
   ```bash
   cd src/InvestTax.Lambda.EmailSender
   dotnet build
   ```

#### Expected Output
```
Success Email Headers:
From: noreply@investtax.example.com
To: user@example.com
Subject: InvestTax Calculator - Tax Report 2024 (Job: 550e8400...)

Lambda Output:
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "stage": "EMAIL_SENT",
  "emailMessageId": "010001234567abcd-12345678-1234-1234-1234-123456789abc-000000"
}
```

#### Success Criteria
- [ ] SES client configured correctly
- [ ] Success emails sent with report content
- [ ] Error emails sent with actionable guidance
- [ ] Both HTML and plain text versions included
- [ ] Email delivery confirmed via SES MessageId
- [ ] From address properly configured

#### Troubleshooting
- **Email not verified**: Verify sender email in SES console
- **Access denied**: Check Lambda execution role has SES permissions
- **Email not received**: Check spam folder, verify SES sandbox restrictions
- **HTML rendering issues**: Test email in multiple clients

---

## PHASE 3: ORCHESTRATION AND INTEGRATION

---

### Step 12: Step Functions State Machine Implementation

**Duration**: 4-5 hours  
**Dependencies**: Steps 6-11  
**Prerequisites**: All Lambda functions deployed

#### Objective
Create Step Functions state machine to orchestrate the 8-stage processing workflow with error handling and retry logic.

#### Technology Stack
- **Orchestration**: AWS Step Functions (Standard Workflow)
- **Definition**: Amazon States Language (ASL) JSON
- **Integration**: Lambda function invocations

#### Actions

1. **Create State Machine Definition**

   Create `infrastructure/step-functions/workflow-definition.json`:
   ```json
   {
     "Comment": "InvestTax Calculator - Processing Workflow",
     "StartAt": "ExtractMetadata",
     "TimeoutSeconds": 900,
     "States": {
       "ExtractMetadata": {
         "Type": "Task",
         "Comment": "Extract metadata from S3 event and create job record",
         "Resource": "arn:aws:states:::dynamodb:putItem",
         "Parameters": {
           "TableName": "${JobsTableName}",
           "Item": {
             "job_id": {
               "S.$": "$.jobId"
             },
             "email": {
               "S.$": "$.email"
             },
             "upload_time": {
               "S.$": "$$.State.EnteredTime"
             },
             "status": {
               "S": "PENDING"
             },
             "stage": {
               "S": "METADATA_EXTRACTED"
             },
             "file_key": {
               "S.$": "$.fileKey"
             },
             "year": {
               "N.$": "States.Format('{}', $.year)"
             }
           }
         },
         "ResultPath": "$.dynamoResult",
         "Next": "ValidateCSV",
         "Catch": [
           {
             "ErrorEquals": ["States.ALL"],
             "ResultPath": "$.error",
             "Next": "HandleMetadataError"
           }
         ]
       },
       
       "ValidateCSV": {
         "Type": "Task",
         "Comment": "Validate CSV structure and data types",
         "Resource": "${ValidatorLambdaArn}",
         "Retry": [
           {
             "ErrorEquals": [
               "Lambda.ServiceException",
               "Lambda.AWSLambdaException",
               "Lambda.SdkClientException"
             ],
             "IntervalSeconds": 2,
             "MaxAttempts": 3,
             "BackoffRate": 2.0
           }
         ],
         "Next": "CheckValidationResult",
         "Catch": [
           {
             "ErrorEquals": ["States.ALL"],
             "ResultPath": "$.error",
             "Next": "UpdateJobFailed"
           }
         ]
       },
       
       "CheckValidationResult": {
         "Type": "Choice",
         "Comment": "Check if validation passed",
         "Choices": [
           {
             "Variable": "$.stage",
             "StringEquals": "VALIDATION_FAILED",
             "Next": "SendValidationErrorEmail"
           }
         ],
         "Default": "NormalizeData"
       },
       
       "SendValidationErrorEmail": {
         "Type": "Task",
         "Resource": "${EmailSenderLambdaArn}",
         "Next": "UpdateJobFailed"
       },
       
       "NormalizeData": {
         "Type": "Task",
         "Comment": "Normalize dates, currencies, and numbers",
         "Resource": "${NormalizerLambdaArn}",
         "Retry": [
           {
             "ErrorEquals": [
               "Lambda.ServiceException",
               "Lambda.AWSLambdaException",
               "Lambda.SdkClientException"
             ],
             "IntervalSeconds": 2,
             "MaxAttempts": 3,
             "BackoffRate": 2.0
           }
         ],
         "Next": "FetchNBPRates",
         "Catch": [
           {
             "ErrorEquals": ["States.ALL"],
             "ResultPath": "$.error",
             "Next": "UpdateJobFailed"
           }
         ]
       },
       
       "FetchNBPRates": {
         "Type": "Task",
         "Comment": "Fetch exchange rates from NBP API",
         "Resource": "${NBPClientLambdaArn}",
         "Retry": [
           {
             "ErrorEquals": [
               "Lambda.ServiceException",
               "Lambda.AWSLambdaException",
               "Lambda.SdkClientException"
             ],
             "IntervalSeconds": 2,
             "MaxAttempts": 3,
             "BackoffRate": 2.0
           }
         ],
         "Next": "CheckRateFetchResult",
         "Catch": [
           {
             "ErrorEquals": ["States.ALL"],
             "ResultPath": "$.error",
             "Next": "SendRateErrorEmail"
           }
         ]
       },
       
       "CheckRateFetchResult": {
         "Type": "Choice",
         "Comment": "Check if rates were fetched successfully",
         "Choices": [
           {
             "Variable": "$.stage",
             "StringEquals": "RATE_FETCH_ERROR",
             "Next": "SendRateErrorEmail"
           }
         ],
         "Default": "CalculateTax"
       },
       
       "SendRateErrorEmail": {
         "Type": "Task",
         "Resource": "${EmailSenderLambdaArn}",
         "Next": "UpdateJobFailed"
       },
       
       "CalculateTax": {
         "Type": "Task",
         "Comment": "Calculate tax using FIFO matching",
         "Resource": "${CalculatorLambdaArn}",
         "Retry": [
           {
             "ErrorEquals": [
               "Lambda.ServiceException",
               "Lambda.AWSLambdaException",
               "Lambda.SdkClientException"
             ],
             "IntervalSeconds": 2,
             "MaxAttempts": 3,
             "BackoffRate": 2.0
           }
         ],
         "Next": "GenerateReport",
         "Catch": [
           {
             "ErrorEquals": ["States.ALL"],
             "ResultPath": "$.error",
             "Next": "UpdateJobFailed"
           }
         ]
       },
       
       "GenerateReport": {
         "Type": "Task",
         "Comment": "Generate human-readable report",
         "Resource": "${ReportGeneratorLambdaArn}",
         "Retry": [
           {
             "ErrorEquals": [
               "Lambda.ServiceException",
               "Lambda.AWSLambdaException",
               "Lambda.SdkClientException"
             ],
             "IntervalSeconds": 2,
             "MaxAttempts": 3,
             "BackoffRate": 2.0
           }
         ],
         "Next": "SendSuccessEmail",
         "Catch": [
           {
             "ErrorEquals": ["States.ALL"],
             "ResultPath": "$.error",
             "Next": "UpdateJobFailed"
           }
         ]
       },
       
       "SendSuccessEmail": {
         "Type": "Task",
         "Comment": "Send success email with report",
         "Resource": "${EmailSenderLambdaArn}",
         "Retry": [
           {
             "ErrorEquals": [
               "Lambda.ServiceException",
               "Lambda.AWSLambdaException",
               "Lambda.SdkClientException"
             ],
             "IntervalSeconds": 2,
             "MaxAttempts": 2,
             "BackoffRate": 2.0
           }
         ],
         "Next": "UpdateJobSuccess",
         "Catch": [
           {
             "ErrorEquals": ["States.ALL"],
             "ResultPath": "$.error",
             "Next": "UpdateJobFailed"
           }
         ]
       },
       
       "UpdateJobSuccess": {
         "Type": "Task",
         "Comment": "Update job status to SUCCESS",
         "Resource": "arn:aws:states:::dynamodb:updateItem",
         "Parameters": {
           "TableName": "${JobsTableName}",
           "Key": {
             "job_id": {
               "S.$": "$.jobId"
             }
           },
           "UpdateExpression": "SET #status = :status, #stage = :stage, #completed = :completed, #gain = :gain, #tax = :tax",
           "ExpressionAttributeNames": {
             "#status": "status",
             "#stage": "stage",
             "#completed": "completed_time",
             "#gain": "total_gain_pln",
             "#tax": "total_tax_pln"
           },
           "ExpressionAttributeValues": {
             ":status": {
               "S": "SUCCESS"
             },
             ":stage": {
               "S": "COMPLETED"
             },
             ":completed": {
               "S.$": "$$.State.EnteredTime"
             },
             ":gain": {
               "N.$": "States.Format('{}', $.totalGainPLN)"
             },
             ":tax": {
               "N.$": "States.Format('{}', $.totalTaxPLN)"
             }
           }
         },
         "End": true
       },
       
       "UpdateJobFailed": {
         "Type": "Task",
         "Comment": "Update job status to FAILED",
         "Resource": "arn:aws:states:::dynamodb:updateItem",
         "Parameters": {
           "TableName": "${JobsTableName}",
           "Key": {
             "job_id": {
               "S.$": "$.jobId"
             }
           },
           "UpdateExpression": "SET #status = :status, #stage = :stage, #error = :error",
           "ExpressionAttributeNames": {
             "#status": "status",
             "#stage": "stage",
             "#error": "error_message"
           },
           "ExpressionAttributeValues": {
             ":status": {
               "S": "FAILED"
             },
             ":stage": {
               "S.$": "$.stage"
             },
             ":error": {
               "S.$": "$.errorMessage"
             }
           }
         },
         "End": true
       },
       
       "HandleMetadataError": {
         "Type": "Pass",
         "Comment": "Cannot send email without extracting metadata",
         "Result": {
           "error": "Failed to extract metadata"
         },
         "End": true
       }
     }
   }
   ```

2. **Update CDK Stack to Include State Machine**

   Add to `infrastructure/cdk/lib/investtax-stack.ts`:
   ```typescript
   // Read state machine definition
   const stateMachineDefinition = fs.readFileSync(
     path.join(__dirname, '../../step-functions/workflow-definition.json'),
     'utf-8'
   );
   
   // Replace placeholders with actual ARNs
   const definitionWithArns = stateMachineDefinition
     .replace('${JobsTableName}', jobsTable.tableName)
     .replace(/\${ValidatorLambdaArn}/g, validatorLambda.functionArn)
     .replace(/\${NormalizerLambdaArn}/g, normalizerLambda.functionArn)
     .replace(/\${NBPClientLambdaArn}/g, nbpClientLambda.functionArn)
     .replace(/\${CalculatorLambdaArn}/g, calculatorLambda.functionArn)
     .replace(/\${ReportGeneratorLambdaArn}/g, reportGeneratorLambda.functionArn)
     .replace(/\${EmailSenderLambdaArn}/g, emailSenderLambda.functionArn);
   
   // Create state machine
   const stateMachine = new sfn.StateMachine(this, 'ProcessingWorkflow', {
     stateMachineName: `InvestTax-Workflow-${props.stage}`,
     definitionBody: sfn.DefinitionBody.fromString(definitionWithArns),
     timeout: cdk.Duration.minutes(15),
     logs: {
       destination: new logs.LogGroup(this, 'StateMachineLogGroup', {
         logGroupName: `/aws/vendedlogs/states/investtax-workflow-${props.stage}`,
         retention: logs.RetentionDays.ONE_MONTH,
         removalPolicy: cdk.RemovalPolicy.DESTROY
       }),
       level: sfn.LogLevel.ALL
     },
     tracingEnabled: true
   });
   
   // Grant permissions
   jobsTable.grantReadWriteData(stateMachine);
   validatorLambda.grantInvoke(stateMachine);
   normalizerLambda.grantInvoke(stateMachine);
   nbpClientLambda.grantInvoke(stateMachine);
   calculatorLambda.grantInvoke(stateMachine);
   reportGeneratorLambda.grantInvoke(stateMachine);
   emailSenderLambda.grantInvoke(stateMachine);
   ```

3. **Create State Machine Starter Lambda (Triggered by S3)**

   Create `infrastructure/step-functions/starter-lambda.ts`:
   ```typescript
   import { S3Event } from 'aws-lambda';
   import { SFNClient, StartExecutionCommand } from '@aws-sdk/client-sfn';
   import { S3Client, HeadObjectCommand } from '@aws-sdk/client-s3';
   
   const sfnClient = new SFNClient({});
   const s3Client = new S3Client({});
   const stateMachineArn = process.env.STATE_MACHINE_ARN!;
   
   export const handler = async (event: S3Event) => {
     for (const record of event.Records) {
       const bucket = record.s3.bucket.name;
       const key = decodeURIComponent(record.s3.object.key.replace(/\+/g, ' '));
       
       // Extract job ID from filename
       const jobId = key.split('/').pop()!.replace('.csv', '');
       
       // Get email from S3 object metadata
       const headResponse = await s3Client.send(new HeadObjectCommand({
         Bucket: bucket,
         Key: key
       }));
       
       const email = headResponse.Metadata?.['email'] || '';
       const year = parseInt(key.split('/')[0]) || new Date().getFullYear();
       
       if (!email) {
         console.error(`No email metadata found for object: ${key}`);
         continue;
       }
       
       // Start Step Functions execution
       const input = {
         jobId,
         email,
         fileKey: key,
         year,
         bucket
       };
       
       const command = new StartExecutionCommand({
         stateMachineArn,
         name: `execution-${jobId}`,
         input: JSON.stringify(input)
       });
       
       try {
         const response = await sfnClient.send(command);
         console.log(`Started execution: ${response.executionArn}`);
       } catch (error) {
         console.error(`Failed to start execution: ${error}`);
         throw error;
       }
     }
   };
   ```

4. **Deploy State Machine**
   ```bash
   cd infrastructure/cdk
   npm run build
   npx cdk synth -c stage=dev
   npx cdk deploy -c stage=dev
   ```

5. **Test State Machine Execution**
   ```bash
   # Create test input
   cat > test-input.json <<EOF
   {
     "jobId": "test-123",
     "email": "test@example.com",
     "fileKey": "2024/test-123.csv",
     "year": 2024,
     "bucket": "investtax-upload-dev"
   }
   EOF
   
   # Start execution
   aws stepfunctions start-execution \
     --state-machine-arn <state-machine-arn> \
     --name test-execution-1 \
     --input file://test-input.json
   
   # Check execution status
   aws stepfunctions describe-execution \
     --execution-arn <execution-arn>
   ```

#### Expected Output
```
State Machine Created:
Name: InvestTax-Workflow-dev
Type: STANDARD
Timeout: 15 minutes
States: 15
Transitions: 20+

Visual Workflow:
ExtractMetadata → ValidateCSV → CheckValidation → NormalizeData →
FetchRates → CheckRates → CalculateTax → GenerateReport →
SendEmail → UpdateJobSuccess

Error Paths:
ValidateCSV (FAIL) → SendValidationErrorEmail → UpdateJobFailed
FetchRates (FAIL) → SendRateErrorEmail → UpdateJobFailed
Any (ERROR) → UpdateJobFailed
```

#### Success Criteria
- [ ] State machine definition valid JSON
- [ ] All Lambda ARNs correctly substituted
- [ ] DynamoDB table name substituted
- [ ] State machine deploys successfully
- [ ] CloudWatch logging configured
- [ ] X-Ray tracing enabled
- [ ] Test execution completes successfully
- [ ] Error paths trigger correctly

#### Troubleshooting
- **Invalid state machine definition**: Validate JSON with ASL validator
- **Permission errors**: Check IAM role has InvokeFunction permissions
- **Timeout errors**: Increase individual Lambda timeouts or state machine timeout
- **Execution not starting**: Check S3 event trigger configuration

---

### Step 13: S3 Event Trigger Integration

**Duration**: 2-3 hours  
**Dependencies**: Step 12  
**Prerequisites**: State machine deployed, S3 buckets created

#### Objective
Configure S3 bucket to trigger Step Functions workflow when CSV files are uploaded.

#### Technology Stack
- **Event Source**: Amazon S3 ObjectCreated events
- **Event Router**: Amazon EventBridge
- **Target**: AWS Step Functions state machine

#### Actions

1. **Enable EventBridge Notifications on S3 Bucket**

   Add to CDK stack:
   ```typescript
   // Enable EventBridge notifications
   uploadBucket.enableEventBridgeNotification();
   ```

2. **Create EventBridge Rule**

   Add to CDK stack:
   ```typescript
   import * as events from 'aws-cdk-lib/aws-events';
   import * as targets from 'aws-cdk-lib/aws-events-targets';
   
   const s3UploadRule = new events.Rule(this, 'S3UploadRule', {
     ruleName: `investtax-s3-upload-${props.stage}`,
     eventPattern: {
       source: ['aws.s3'],
       detailType: ['Object Created'],
       detail: {
         bucket: { name: [uploadBucket.bucketName] },
         object: { key: [{ suffix: '.csv' }] }
       }
     }
   });
   
   s3UploadRule.addTarget(new targets.SfnStateMachine(stateMachine));
   ```

3. **Test Event Trigger**

   Create test script `scripts/test-upload.ps1`:
   ```powershell
   param(
       [string]$BucketName = "investtax-upload-dev",
       [string]$Email = "test@example.com"
   )
   
   $year = Get-Date -Format "yyyy"
   $jobId = [guid]::NewGuid().ToString()
   $key = "$year/$jobId.csv"
   
   aws s3 cp "test-data/sample.csv" "s3://$BucketName/$key" `
       --metadata "email=$Email" `
       --region eu-central-1
   
   Write-Host "Upload complete. Job ID: $jobId"
   ```

#### Success Criteria
- [ ] EventBridge notifications enabled on S3
- [ ] EventBridge rule filters CSV files
- [ ] State machine triggered on upload
- [ ] Test upload starts workflow

#### Troubleshooting
- **State machine not triggered**: Check EventBridge rule pattern
- **Missing email**: Verify metadata attached during upload

---

### Step 14: End-to-End Integration Testing

**Duration**: 3-4 hours  
**Dependencies**: Step 13  
**Prerequisites**: Complete workflow deployed

#### Objective
Perform comprehensive end-to-end testing with various scenarios.

#### Technology Stack
- **Testing**: Manual testing with documented test cases
- **Monitoring**: CloudWatch Logs, Step Functions console

#### Actions

1. **Create Test Data Set**

   Create test cases:
   - **Test Case 1**: Simple success (2 transactions)
   - **Test Case 2**: Multiple stocks (4+ transactions)
   - **Test Case 3**: Partial fills (FIFO with partials)
   - **Test Case 4**: Validation error (missing ISIN)
   - **Test Case 5**: Validation error (invalid date)

2. **Create Test Execution Script**

   Create `scripts/run-integration-tests.ps1`:
   ```powershell
   $testCases = @(
       @{Name="Simple Success"; File="test-1.csv"; ShouldSucceed=$true},
       @{Name="Validation Error"; File="test-4.csv"; ShouldSucceed=$false}
   )
   
   foreach ($testCase in $testCases) {
       $jobId = [guid]::NewGuid().ToString()
       $key = "2024/$jobId.csv"
       
       aws s3 cp "test-data/$($testCase.File)" "s3://$BucketName/$key" `
           --metadata "email=$Email"
       
       Write-Host "Test: $($testCase.Name) - Job ID: $jobId"
       Start-Sleep -Seconds 60
   }
   ```

3. **Create Test Checklist**

   Document in `test-data/TEST-CHECKLIST.md`:
   - Pre-test setup verification
   - Test case execution steps
   - Expected outcomes
   - CloudWatch logs verification
   - Performance benchmarks

#### Success Criteria
- [ ] All success cases complete end-to-end
- [ ] Tax calculations match expected values
- [ ] Error cases fail gracefully with emails
- [ ] Performance < 5 minutes for typical files

---

### Step 15: Unit Test Implementation

**Duration**: 4-5 hours  
**Dependencies**: Steps 6-11  
**Prerequisites**: All Lambda functions implemented

#### Objective
Implement comprehensive unit tests with 80% code coverage.

#### Technology Stack
- **Test Framework**: xUnit 2.9.x
- **Mocking**: Moq 4.20.x
- **Coverage**: Coverlet 6.x
- **Assertions**: FluentAssertions 6.12.x

#### Actions

1. **Add Test Dependencies**
   ```bash
   cd tests/InvestTax.Core.Tests
   dotnet add package xUnit --version 2.9.0
   dotnet add package Moq --version 4.20.70
   dotnet add package FluentAssertions --version 6.12.0
   dotnet add package coverlet.collector --version 6.0.2
   ```

2. **Create FIFO Calculator Tests**

   Create `tests/InvestTax.Core.Tests/Services/TaxCalculatorServiceTests.cs`:
   ```csharp
   public class TaxCalculatorServiceTests
   {
       [Fact]
       public void CalculateTax_SimpleBuyAndSell_CalculatesCorrectGain()
       {
           // Test implementation
       }
       
       [Fact]
       public void CalculateTax_PartialFill_MatchesCorrectly()
       {
           // Test implementation
       }
       
       [Fact]
       public void CalculateTax_SellWithoutBuy_ThrowsException()
       {
           // Test implementation
       }
   }
   ```

3. **Create Validation Tests**

   Create validator tests for CSV validation logic.

4. **Run Tests with Coverage**
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
   ```

#### Success Criteria
- [ ] All unit tests pass
- [ ] Code coverage >= 80%
- [ ] Core business logic >= 90% coverage
- [ ] Tests run in < 30 seconds

---

### Step 16: Integration Test Scenarios

**Duration**: 2-3 hours  
**Dependencies**: Step 15  
**Prerequisites**: Unit tests complete

#### Objective
Create reusable integration test scenarios and test data.

#### Actions

1. **Create Test Data Generator**

   Create `scripts/generate-test-data.ps1`:
   ```powershell
   function Generate-TestCase {
       param($Name, $Transactions, $ExpectedGain)
       
       # Generate CSV file
       $csv = @"
   Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
   "@
       
       foreach ($tx in $Transactions) {
           $csv += "`n$tx"
       }
       
       $csv | Out-File "test-data/$Name.csv" -Encoding UTF8
       
       # Generate expected output
       @{
           Name = $Name
           ExpectedGain = $ExpectedGain
       } | ConvertTo-Json | Out-File "test-data/$Name.expected.json"
   }
   ```

2. **Document Test Scenarios**

   Create `test-data/SCENARIOS.md` documenting:
   - Test purpose
   - Input data
   - Expected outputs
   - Edge cases covered

#### Success Criteria
- [ ] 10+ test scenarios documented
- [ ] Test data generator script created
- [ ] Expected outputs documented

---

### Step 17: Test Data Generation

**Duration**: 2 hours  
**Dependencies**: Step 16  
**Prerequisites**: Test scenarios defined

#### Objective
Generate comprehensive test data covering all edge cases.

#### Actions

1. **Generate Standard Test Files**
   ```powershell
   .\\scripts\\generate-test-data.ps1 -Scenarios All
   ```

2. **Create Edge Case Tests**
   - Year boundaries
   - Multiple currencies
   - Large files (10,000+ transactions)
   - Weekend/holiday dates for NBP

3. **Validate Test Data**
   ```powershell
   .\\scripts\\validate-test-data.ps1
   ```

#### Success Criteria
- [ ] 20+ test CSV files generated
- [ ] Edge cases covered
- [ ] All files pass validation

---

### Step 18: GitHub Actions Build Pipeline

**Duration**: 3-4 hours  
**Dependencies**: Step 15  
**Prerequisites**: Tests implemented

#### Objective
Create CI/CD pipeline for automated builds and tests.

#### Technology Stack
- **CI/CD**: GitHub Actions
- **.NET SDK**: 10.x
- **Node.js**: 20.x (for CDK)

#### Actions

1. **Create Workflow File**

   Create `.github/workflows/build.yml`:
   ```yaml
   name: Build and Test
   
   on:
     push:
       branches: [ main, develop ]
     pull_request:
       branches: [ main ]
   
   jobs:
     build:
       runs-on: ubuntu-latest
       
       steps:
       - uses: actions/checkout@v4
       
       - name: Setup .NET
         uses: actions/setup-dotnet@v4
         with:
           dotnet-version: '10.0.x'
       
       - name: Restore dependencies
         run: dotnet restore
       
       - name: Build
         run: dotnet build --no-restore --configuration Release
       
       - name: Test
         run: dotnet test --no-build --configuration Release /p:CollectCoverage=true
       
       - name: Upload coverage
         uses: codecov/codecov-action@v3
         with:
           files: ./tests/**/coverage.opencover.xml
   
     cdk-synth:
       runs-on: ubuntu-latest
       
       steps:
       - uses: actions/checkout@v4
       
       - name: Setup Node.js
         uses: actions/setup-node@v4
         with:
           node-version: '20'
       
       - name: Install CDK dependencies
         working-directory: infrastructure/cdk
         run: npm ci
       
       - name: CDK Synth
         working-directory: infrastructure/cdk
         run: npx cdk synth -c stage=dev
   ```

2. **Test Workflow Locally**
   ```bash
   # Install act for local testing
   gh act push
   ```

#### Success Criteria
- [ ] Workflow runs on push/PR
- [ ] All tests pass in CI
- [ ] Code coverage reported
- [ ] CDK synthesizes successfully

---

### Step 19: Deployment Scripts and Runbooks

**Duration**: 3 hours  
**Dependencies**: Step 18  
**Prerequisites**: CI/CD pipeline working

#### Objective
Create deployment automation and operational documentation.

#### Actions

1. **Create Deployment Script**

   Create `scripts/deploy.ps1`:
   ```powershell
   param(
       [Parameter(Mandatory=$true)]
       [ValidateSet("dev", "staging", "prod")]
       [string]$Stage,
       
       [switch]$SkipTests
   )
   
   Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
   Write-Host " InvestTax Calculator Deployment" -ForegroundColor Cyan
   Write-Host " Stage: $Stage" -ForegroundColor Cyan
   Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
   
   # Run tests
   if (-not $SkipTests) {
       Write-Host "`n[1/5] Running tests..." -ForegroundColor Yellow
       dotnet test --configuration Release
       if ($LASTEXITCODE -ne 0) {
           Write-Host "Tests failed!" -ForegroundColor Red
           exit 1
       }
   }
   
   # Build Lambdas
   Write-Host "`n[2/5] Building Lambda functions..." -ForegroundColor Yellow
   $lambdas = @(
       "InvestTax.Lambda.Validator",
       "InvestTax.Lambda.Normalizer",
       "InvestTax.Lambda.NBPClient",
       "InvestTax.Lambda.Calculator",
       "InvestTax.Lambda.ReportGenerator",
       "InvestTax.Lambda.EmailSender"
   )
   
   foreach ($lambda in $lambdas) {
       Write-Host "  Building $lambda..."
       dotnet publish "src/$lambda" -c Release -o "src/$lambda/publish"
   }
   
   # Deploy CDK
   Write-Host "`n[3/5] Deploying CDK stack..." -ForegroundColor Yellow
   cd infrastructure/cdk
   npm ci
   npx cdk deploy -c stage=$Stage --require-approval never
   cd ../..
   
   # Verify deployment
   Write-Host "`n[4/5] Verifying deployment..." -ForegroundColor Yellow
   $stateMachineArn = aws cloudformation describe-stacks `
       --stack-name "InvestTaxStack-$Stage" `
       --query "Stacks[0].Outputs[?OutputKey=='StateMachineArn'].OutputValue" `
       --output text
   
   Write-Host "State Machine ARN: $stateMachineArn"
   
   # Run smoke test
   Write-Host "`n[5/5] Running smoke test..." -ForegroundColor Yellow
   .\\scripts\\smoke-test.ps1 -Stage $Stage
   
   Write-Host "`n✓ Deployment complete!" -ForegroundColor Green
   ```

2. **Create Rollback Script**

   Create `scripts/rollback.ps1`:
   ```powershell
   param(
       [Parameter(Mandatory=$true)]
       [string]$Stage,
       
       [Parameter(Mandatory=$true)]
       [string]$PreviousVersion
   )
   
   Write-Host "Rolling back to version: $PreviousVersion" -ForegroundColor Yellow
   
   cd infrastructure/cdk
   git checkout $PreviousVersion
   npx cdk deploy -c stage=$Stage --require-approval never
   ```

3. **Create Operational Runbook**

   Create `docs/RUNBOOK.md`:
   ```markdown
   # InvestTax Calculator - Operational Runbook
   
   ## Deployment
   
   ### Prerequisites
   - AWS CLI configured
   - .NET 10 SDK installed
   - Node.js 20 installed
   - IAM permissions for CloudFormation, Lambda, S3, DynamoDB, SES, Step Functions
   
   ### Deployment Steps
   
   1. Deploy to dev:
      ```powershell
      .\\scripts\\deploy.ps1 -Stage dev
      ```
   
   2. Deploy to prod:
      ```powershell
      .\\scripts\\deploy.ps1 -Stage prod
      ```
   
   ## Monitoring
   
   ### Key Metrics
   - Step Functions execution success rate
   - Lambda error rates
   - Email delivery rate
   - Average processing time
   
   ### CloudWatch Dashboards
   - View: https://console.aws.amazon.com/cloudwatch/home?region=eu-central-1#dashboards
   
   ## Troubleshooting
   
   ### Issue: Workflow not triggered
   **Symptoms**: File uploaded but no Step Functions execution
   **Diagnosis**:
   ```powershell
   aws events list-rules --name-prefix investtax
   ```
   **Resolution**: Check EventBridge rule is enabled
   
   ### Issue: NBP API timeout
   **Symptoms**: Rate fetch stage fails
   **Diagnosis**: Check CloudWatch logs for NBP Client Lambda
   **Resolution**: Increase Lambda timeout, check NBP API status
   
   ### Issue: Email not delivered
   **Symptoms**: Workflow completes but no email
   **Diagnosis**:
   ```powershell
   aws ses get-send-statistics
   ```
   **Resolution**: Verify SES sending limits, check bounce rate
   
   ## Rollback Procedure
   
   1. Identify previous stable version:
      ```powershell
      git log --oneline
      ```
   
   2. Execute rollback:
      ```powershell
      .\\scripts\\rollback.ps1 -Stage prod -PreviousVersion <commit-hash>
      ```
   
   3. Verify:
      ```powershell
      .\\scripts\\smoke-test.ps1 -Stage prod
      ```
   ```

#### Success Criteria
- [ ] Deployment script automates full deployment
- [ ] Rollback script implemented and tested
- [ ] Runbook covers common scenarios
- [ ] Smoke tests verify deployment

---

### Step 20: User Documentation and Handoff

**Duration**: 4 hours  
**Dependencies**: Step 19  
**Prerequisites**: System fully operational

#### Objective
Create user documentation and complete project handoff.

#### Actions

1. **Create User Guide**

   Create `docs/USER-GUIDE.md`:
   ```markdown
   # InvestTax Calculator - User Guide
   
   ## Overview
   
   InvestTax Calculator is an automated system for calculating capital gains taxes on Polish stock transactions using the FIFO (First-In, First-Out) method.
   
   ## How to Use
   
   ### Step 1: Prepare Your CSV File
   
   Create a pipe-delimited CSV file with the following columns:
   
   ```
   Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
   ```
   
   **Requirements**:
   - Use pipe (|) as delimiter
   - Include all transactions for a single tax year
   - Dates in ISO 8601 format (YYYY-MM-DDTHH:MM:SSZ)
   - All transactions must be "Market buy" or "Market sell"
   
   **Example**:
   ```csv
   Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
   Market buy|2024-01-15T10:00:00Z|US0378331005|AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|
   Market sell|2024-06-20T14:00:00Z|US0378331005|AAPL|Apple Inc.|10|170.00|USD|4.05|1700.00|6885.00|
   ```
   
   ### Step 2: Upload to S3
   
   Upload your CSV file to the S3 bucket with your email in metadata:
   
   ```powershell
   aws s3 cp your-file.csv s3://investtax-upload-prod/2024/your-unique-id.csv `
       --metadata "email=your-email@example.com"
   ```
   
   ### Step 3: Wait for Email
   
   You will receive an email within 5 minutes containing:
   - Total capital gain (PLN)
   - Total tax owed (19%)
   - Detailed transaction breakdown
   - FIFO matching details
   
   ## Understanding Your Report
   
   ### Summary Section
   - **Total Capital Gain**: Total profit in PLN
   - **Tax Rate**: 19% flat rate (Polish law)
   - **Total Tax Owed**: Amount to report on PIT-38
   
   ### Transaction Details
   Each matched buy-sell pair shows:
   - Buy date, shares, price (original currency + PLN)
   - Sell date, shares, price (original currency + PLN)
   - Gain/loss for that specific match
   
   ### Methodology
   - **FIFO**: First shares bought are first shares sold
   - **Exchange Rates**: NBP (National Bank of Poland) official rates
   - **Rounding**: All PLN amounts rounded to 2 decimals
   
   ## Troubleshooting
   
   ### Error: "Email metadata not found"
   **Solution**: Include --metadata "email=your@email.com" when uploading
   
   ### Error: "Invalid date format"
   **Solution**: Use ISO 8601 format: YYYY-MM-DDTHH:MM:SSZ
   
   ### Error: "Missing required column"
   **Solution**: Ensure all 12 columns are present with exact names
   
   ## Important Disclaimers
   
   - This is NOT official tax advice
   - You must verify all calculations
   - Consult a tax professional before filing
   - You are responsible for accuracy
   ```

2. **Create API Documentation**

   Generate XML documentation comments in all public classes and methods.

3. **Create Handoff Checklist**

   Create `docs/HANDOFF-CHECKLIST.md`:
   ```markdown
   # Project Handoff Checklist
   
   ## Code & Infrastructure
   - [ ] All source code committed to repository
   - [ ] CDK infrastructure code complete
   - [ ] All Lambda functions implemented
   - [ ] Step Functions state machine deployed
   - [ ] Unit tests achieve 80%+ coverage
   - [ ] Integration tests documented
   
   ## Deployment
   - [ ] Dev environment deployed and tested
   - [ ] Prod environment deployed
   - [ ] CI/CD pipeline operational
   - [ ] Deployment scripts tested
   - [ ] Rollback procedure documented
   
   ## Documentation
   - [ ] User Guide complete
   - [ ] Implementation Plan finalized
   - [ ] Operational Runbook created
   - [ ] Architecture diagrams up-to-date
   - [ ] API documentation generated
   - [ ] Test scenarios documented
   
   ## AWS Resources
   - [ ] S3 buckets created and configured
   - [ ] DynamoDB tables created
   - [ ] Lambda functions deployed
   - [ ] Step Functions state machine operational
   - [ ] EventBridge rules configured
   - [ ] SES email verified
   - [ ] CloudWatch dashboards created
   - [ ] IAM roles properly scoped
   
   ## Testing
   - [ ] Unit tests pass (80%+ coverage)
   - [ ] Integration tests pass
   - [ ] End-to-end tests successful
   - [ ] Load testing completed (if applicable)
   - [ ] Error scenarios tested
   
   ## Monitoring & Operations
   - [ ] CloudWatch alarms configured
   - [ ] Logging properly configured
   - [ ] Metrics collected
   - [ ] Troubleshooting guide complete
   
   ## Security
   - [ ] No secrets in source code
   - [ ] IAM roles follow least privilege
   - [ ] S3 buckets not publicly accessible
   - [ ] Encryption at rest enabled
   - [ ] Encryption in transit enforced
   
   ## Cost Management
   - [ ] Budget alerts configured
   - [ ] Resource tagging implemented
   - [ ] Cost optimization reviewed
   
   ## Knowledge Transfer
   - [ ] Team walkthrough completed
   - [ ] Q&A session conducted
   - [ ] Support contacts documented
   ```

4. **Prepare Demo**

   Create demo script for stakeholder presentation:
   - Live upload demonstration
   - CloudWatch logs walkthrough
   - Step Functions execution visualization
   - Email receipt demonstration

#### Success Criteria
- [ ] User guide covers all scenarios
- [ ] Handoff checklist 100% complete
- [ ] Demo successfully presented
- [ ] All documentation reviewed and approved
- [ ] Team trained on system operation

---

## Testing Strategy

**Framework**: xUnit 2.6.x  
**Mocking**: Moq 4.20.x  
**Coverage**: Coverlet 6.x  

**Target Coverage**: 80% for MVP

**Test Categories**:
1. **Core Logic Tests** (InvestTax.Core.Tests)
   - FIFO matching algorithm
   - Tax calculation logic
   - Currency conversion
   - Model validation

2. **Lambda Handler Tests** (InvestTax.Lambda.Tests)
   - Input/output mapping
   - Error handling
   - Integration with services

**Test Data**: Sample CSV files with known expected outputs

---

## Deployment Process

### Manual Deployment (MVP)

1. **Build Lambda Packages**:
   ```bash
   cd src/InvestTax.Lambda.Validator
   dotnet publish -c Release -o publish
   ```

2. **Deploy Infrastructure**:
   ```bash
   cd infrastructure/cdk
   npx cdk deploy -c stage=dev
   ```

3. **Verify Deployment**:
   - Check CloudFormation stack status
   - Test Lambda functions individually
   - Upload test CSV file
   - Verify email delivery

---

## Post-MVP Enhancements (Phase 2)

### Performance Optimizations
- **NBP Rate Caching**: DynamoDB cache with 30-day TTL
- **Parallel Processing**: Concurrent NBP API calls
- **Batch Operations**: Process multiple ISINs in parallel

### Enhanced Features
- **HTML Email Reports**: Rich formatting with charts
- **S3 Archive Bucket**: Long-term retention with Glacier transition
- **CloudWatch Dashboards**: Real-time monitoring
- **AWS X-Ray**: Distributed tracing
- **Dead Letter Queues**: Failed message handling

### Infrastructure
- **Multi-Region**: Cross-region replication
- **VPC Deployment**: Enhanced security
- **WAF Integration**: API protection

---

## Success Criteria and Handoff

### MVP Completion Criteria

- [ ] All 6 Lambda functions deployed and operational
- [ ] Step Functions workflow executes end-to-end
- [ ] S3 upload triggers workflow automatically
- [ ] Tax calculation produces accurate results (validated against test data)
- [ ] Email delivery confirms successful processing
- [ ] Unit tests achieve 80% coverage
- [ ] GitHub Actions build pipeline passes
- [ ] Local development environment functional
- [ ] Documentation complete

### Handoff Deliverables

1. **Code Repository**: Complete .NET solution with all projects
2. **Infrastructure Code**: CDK stack ready for deployment
3. **Test Suite**: Unit tests with 80% coverage
4. **Local Environment**: Docker Compose setup for development
5. **Documentation**:
   - This implementation plan
   - API documentation (XML comments)
   - Deployment runbook
   - Troubleshooting guide
6. **CI/CD Pipeline**: GitHub Actions workflow

---

## Appendix: Quick Reference

### Key Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Publish Lambda function
dotnet publish -c Release -r linux-x64 --self-contained false

# Start local environment
./scripts/start-local.sh

# Deploy infrastructure
cd infrastructure/cdk && npx cdk deploy -c stage=dev

# Synthesize CDK
npx cdk synth -c stage=dev

# View CDK diff
npx cdk diff -c stage=dev
```

### Environment Variables

**Lambda Functions**:
- `UPLOAD_BUCKET`: S3 upload bucket name
- `PROCESSING_BUCKET`: S3 processing bucket name
- `JOBS_TABLE`: DynamoDB jobs table name
- `NBP_API_URL`: NBP API base URL
- `SES_FROM_EMAIL`: SES sender email
- `STAGE`: Deployment stage (dev/prod)

**Local Development**:
- `AWS_ENDPOINT`: LocalStack endpoint (http://localhost:4566)
- `AWS_REGION`: eu-central-1
- `AWS_ACCESS_KEY_ID`: test
- `AWS_SECRET_ACCESS_KEY`: test

### Useful AWS CLI Commands

```bash
# List S3 buckets
aws s3 ls --endpoint-url=http://localhost:4566

# Query DynamoDB
aws dynamodb scan --table-name InvestTax-Jobs-Local --endpoint-url=http://localhost:4566

# Start Step Functions execution
aws stepfunctions start-execution --state-machine-arn <arn> --input file://test-input.json

# View CloudWatch logs
aws logs tail /aws/lambda/InvestTax-Validator-dev --follow
```

---

**End of Implementation Plan**

This plan is designed to be executed step-by-step by development agents. Each step is self-contained and includes all necessary details for implementation. For questions or clarifications, refer to the architecture documentation in the `docs/architecture/` folder.
