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

**Note**: Due to length constraints, this implementation plan continues with Steps 6-20 covering:
- Step 6-11: Individual Lambda function implementations
- Step 12-14: Step Functions orchestration and integration
- Step 15-17: Unit testing
- Step 18-20: GitHub Actions CI/CD

Each step follows the same detailed format with:
- Duration estimate
- Dependencies
- Technology stack
- Detailed actions with code
- Expected output
- Success criteria
- Troubleshooting tips

The remaining steps maintain consistency with the MVP scope:
- No caching (Phase 2)
- No parallel processing (Phase 2)
- Plain text reports only (Phase 2 adds HTML)
- Sequential Lambda invocations
- Basic error handling

---

## Testing Strategy

### Unit Testing Approach

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
