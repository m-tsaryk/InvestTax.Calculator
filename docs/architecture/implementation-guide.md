# Implementation Guide

[← Back to Index](README.md)

## Overview

This guide provides step-by-step instructions for implementing the InvestTax Calculator, from initial setup through production deployment.

**Technology Note**: This implementation guide uses **.NET 8** for all Lambda functions and **C#** for AWS CDK infrastructure code. All code examples demonstrate .NET patterns including async/await, dependency injection, and strong typing.

---

## Pre-Implementation Checklist

Before starting implementation, verify:

- [ ] **AWS Account Setup**: AWS account provisioned with necessary service quotas
- [ ] **SES Configuration**: Sender email domain verified in Amazon SES
- [ ] **NBP API Access**: NBP API accessible from development environment (test connection)
- [ ] **Development Environment**: Team members set up with:
  - AWS CLI configured with credentials
  - AWS CDK installed (`npm install -g aws-cdk`)
  - .NET 8 SDK installed
  - Git and GitHub access
- [ ] **Test Data**: Sample CSV files prepared (1K, 10K, 100K rows)
- [ ] **Reference Calculator**: Trusted reference available for validating tax calculations

---

## Phase 1: Initial Implementation (Weeks 1-3)

### Week 1: Foundation and Data Validation

#### Day 1-2: Project Setup

```bash
# Create project structure
mkdir investtax-calculator
cd investtax-calculator
git init

# Initialize CDK project
mkdir infrastructure
cd infrastructure
cdk init app --language=csharp
cd ..

# Initialize Lambda functions as .NET class libraries
mkdir -p src
cd src

# Create solution file
dotnet new sln -n InvestTax.Calculator

# Create shared library
dotnet new classlib -n InvestTax.Common
dotnet sln add InvestTax.Common/InvestTax.Common.csproj

# Create Lambda function projects
dotnet new lambda.EmptyFunction -n InvestTax.MetadataExtractor
dotnet new lambda.EmptyFunction -n InvestTax.CsvValidator
dotnet new lambda.EmptyFunction -n InvestTax.DataNormalizer
dotnet new lambda.EmptyFunction -n InvestTax.NbpRateFetcher
dotnet new lambda.EmptyFunction -n InvestTax.TaxCalculator
dotnet new lambda.EmptyFunction -n InvestTax.ReportGenerator
dotnet new lambda.EmptyFunction -n InvestTax.EmailSender

# Add projects to solution
dotnet sln add InvestTax.MetadataExtractor/InvestTax.MetadataExtractor.csproj
dotnet sln add InvestTax.CsvValidator/InvestTax.CsvValidator.csproj
dotnet sln add InvestTax.DataNormalizer/InvestTax.DataNormalizer.csproj
dotnet sln add InvestTax.NbpRateFetcher/InvestTax.NbpRateFetcher.csproj
dotnet sln add InvestTax.TaxCalculator/InvestTax.TaxCalculator.csproj
dotnet sln add InvestTax.ReportGenerator/InvestTax.ReportGenerator.csproj
dotnet sln add InvestTax.EmailSender/InvestTax.EmailSender.csproj

# Install common NuGet packages
cd InvestTax.Common
dotnet add package AWSSDK.S3
dotnet add package AWSSDK.DynamoDBv2
dotnet add package CsvHelper
dotnet add package System.Text.Json
cd ..
```

**Deliverables**:
- [ ] Git repository initialized
- [ ] CDK project structure created
- [ ] .NET solution and projects configured
- [ ] Directory structure established

---

#### Day 3-4: Storage Infrastructure

**File**: `infrastructure/src/Stacks/StorageStack.cs`

```csharp
using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Constructs;
using System.Collections.Generic;

namespace InvestTax.Infrastructure.Stacks
{
    public class StorageStack : Stack
    {
        public Bucket UploadBucket { get; }
        public Bucket ProcessingBucket { get; }

        public StorageStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Upload bucket (user uploads)
            UploadBucket = new Bucket(this, "UploadBucket", new BucketProps
            {
                Encryption = BucketEncryption.S3_MANAGED,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                Versioned = true,
                LifecycleRules = new[]
                {
                    new LifecycleRule
                    {
                        Enabled = true,
                        Transitions = new[]
                        {
                            new Transition
                            {
                                StorageClass = StorageClass.INTELLIGENT_TIERING,
                                TransitionAfter = Duration.Days(30)
                            }
                        }
                    }
                }
            });

            // Processing bucket (intermediate files)
            ProcessingBucket = new Bucket(this, "ProcessingBucket", new BucketProps
            {
                Encryption = BucketEncryption.S3_MANAGED,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                LifecycleRules = new[]
                {
                    new LifecycleRule
                    {
                        Enabled = true,
                        Expiration = Duration.Days(7)  // Auto-delete after 7 days
                    }
                }
            });
        }
    }
}
```

**Deploy**:
```bash
cd infrastructure
cdk bootstrap  # One-time setup
cdk deploy StorageStack
```

**Deliverables**:
- [ ] S3 Upload Bucket created
- [ ] S3 Processing Bucket created
- [ ] Encryption enabled (SSE-S3)
- [ ] Lifecycle policies configured

---

#### Day 5: Database Infrastructure

**File**: `infrastructure/src/Stacks/DatabaseStack.cs`

```csharp
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace InvestTax.Infrastructure.Stacks
{
    public class DatabaseStack : Stack
    {
        public Table JobsTable { get; }

        public DatabaseStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Jobs table (job tracking)
            JobsTable = new Table(this, "JobsTable", new TableProps
            {
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "job_id",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.ON_DEMAND,
                Encryption = TableEncryption.AWS_MANAGED,
                TimeToLiveAttribute = "ttl",  // Auto-cleanup after 30 days
                PointInTimeRecovery = true   // Backup
            });
        }
    }
}
```

**Deploy**:
```bash
cdk deploy DatabaseStack
```

**Deliverables**:
- [ ] DynamoDB Jobs table created
- [ ] On-demand billing configured
- [ ] TTL attribute enabled
- [ ] Point-in-time recovery enabled

---

#### Day 6-7: Metadata Extractor & CSV Validator Lambdas

**File**: `src/InvestTax.MetadataExtractor/Function.cs`

```csharp
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestTax.MetadataExtractor
{
    public class Function
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private const string JobsTableName = "InvestTax-Jobs";

        public Function()
        {
            _s3Client = new AmazonS3Client();
            _dynamoDbClient = new AmazonDynamoDBClient();
        }

        public async Task<JobMetadata> FunctionHandler(S3Event input, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing S3 event for bucket: {input.Bucket}, key: {input.Key}");

            // Download CSV header (first 1000 bytes)
            var request = new GetObjectRequest
            {
                BucketName = input.Bucket,
                Key = input.Key,
                ByteRange = new ByteRange(0, 999)
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            var headerContent = await reader.ReadToEndAsync();

            // Parse CSV header
            using var csvReader = new CsvReader(new StringReader(headerContent), CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            var fieldNames = csvReader.HeaderRecord;

            // Extract year from filename
            var year = ExtractYear(input.Key);
            var jobId = $"{input.Email}_{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}";
            var ttl = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();

            // Create job record in DynamoDB
            var putItemRequest = new PutItemRequest
            {
                TableName = JobsTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    ["job_id"] = new AttributeValue { S = jobId },
                    ["email"] = new AttributeValue { S = input.Email },
                    ["upload_time"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
                    ["file_key"] = new AttributeValue { S = input.Key },
                    ["year"] = new AttributeValue { N = year.ToString() },
                    ["status"] = new AttributeValue { S = "PENDING" },
                    ["ttl"] = new AttributeValue { N = ttl.ToString() }
                }
            };

            await _dynamoDbClient.PutItemAsync(putItemRequest);

            return new JobMetadata
            {
                JobId = jobId,
                Bucket = input.Bucket,
                Key = input.Key,
                Email = input.Email,
                Year = year,
                FieldNames = fieldNames.ToList()
            };
        }

        private int ExtractYear(string filename)
        {
            // Extract year from filename (e.g., 'transactions_2025.csv')
            // Implementation here - can use regex or string parsing
            return 2025;
        }
    }

    public class S3Event
    {
        public string Bucket { get; set; }
        public string Key { get; set; }
        public string Email { get; set; }
    }

    public class JobMetadata
    {
        public string JobId { get; set; }
        public string Bucket { get; set; }
        public string Key { get; set; }
        public string Email { get; set; }
        public int Year { get; set; }
        public List<string> FieldNames { get; set; }
    }
}
```

---

**📝 Important Note on Code Examples Below**:

The remaining Lambda function examples in this guide (CSV Validator through Email Sender) are shown in **Python for conceptual demonstration only**. When implementing your solution, **convert all examples to .NET 8** following the pattern shown in the Metadata Extractor above:

**Key .NET Implementation Patterns**:
1. **Async/Await**: Use `async Task<T>` for all handlers and I/O operations
2. **AWS SDK**: Use AWSSDK.S3, AWSSDK.DynamoDBv2, AWSSDK.SimpleEmail NuGet packages
3. **CSV Parsing**: Use `CsvHelper` library (NuGet)
4. **JSON**: Use `System.Text.Json` for serialization
5. **HTTP Clients**: Use `HttpClient` with `IHttpClientFactory` for NBP API calls
6. **Validation**: Use `FluentValidation` library for business rules
7. **Testing**: Use `xUnit` with `FluentAssertions` and `Moq` for mocking AWS services
8. **Dependency Injection**: Constructor inject IAmazonS3, IAmazonDynamoDB, etc.
9. **Logging**: Use `ILambdaContext.Logger.LogLine()` for structured logging
10. **Models**: Create strongly-typed POCOs for all inputs/outputs

**Lambda Deployment (.NET)**:
```bash
# Build and package Lambda function
cd src/InvestTax.MetadataExtractor
dotnet lambda package --configuration Release --framework net8.0

# Deploy via CDK
cd ../../infrastructure
cdk deploy ComputeStack
```

---

**CSV Validator** (Conceptual - implement in C#):

**File**: `src/csv-validator/handler.py` ← **Convert this to C#**

```python
import boto3
import csv
from io import StringIO

s3 = boto3.client('s3')

REQUIRED_COLUMNS = ['Date', 'ISIN', 'Transaction Type', 'Quantity', 'Price', 'Currency']

def lambda_handler(event, context):
    """Validate CSV structure and required columns."""
    
    bucket = event['bucket']
    key = event['key']
    
    # Download CSV
    response = s3.get_object(Bucket=bucket, Key=key)
    content = response['Body'].read().decode('utf-8')
    
    # Validate
    errors = []
    reader = csv.DictReader(StringIO(content))
    
    # Check required columns
    missing_cols = set(REQUIRED_COLUMNS) - set(reader.fieldnames)
    if missing_cols:
        errors.append(f"Missing required columns: {missing_cols}")
    
    # Validate each row
    for i, row in enumerate(reader, start=2):  # Start at 2 (header is row 1)
        # Date format
        if not is_valid_date(row.get('Date')):
            errors.append(f"Row {i}: Invalid date format")
        
        # Numeric fields
        if not is_numeric(row.get('Quantity')):
            errors.append(f"Row {i}: Quantity must be numeric")
        
        # More validations...
    
    if errors:
        return {'status': 'INVALID', 'errors': errors}
    
    # Write validated CSV to processing bucket
    processing_key = f"validated/{event['job_id']}.csv"
    s3.put_object(Bucket='processing-bucket', Key=processing_key, Body=content)
    
    return {
        **event,
        'status': 'VALID',
        'validated_key': processing_key,
    }

def is_valid_date(date_str):
    # Implementation
    return True

def is_numeric(value):
    try:
        float(value)
        return True
    except:
        return False
```

**Deploy**:
```bash
# Package Lambda functions
cd src/metadata-extractor
zip -r ../../deployment/metadata-extractor.zip .

cd ../csv-validator
zip -r ../../deployment/csv-validator.zip .

# Deploy via CDK (infrastructure/lib/stacks/compute-stack.ts)
cdk deploy ComputeStack
```

**Testing**:
```bash
# Upload test CSV to S3
aws s3 cp test-data/sample-1k.csv s3://upload-bucket/

# Check Lambda execution in CloudWatch Logs
aws logs tail /aws/lambda/MetadataExtractor --follow
```

**Deliverables**:
- [ ] Metadata Extractor Lambda implemented (.NET)
- [ ] CSV Validator Lambda implemented (.NET)
- [ ] Unit tests written (xUnit)
- [ ] Deployed to dev environment
- [ ] Tested with sample CSV files

---

### Week 2: Core Processing Pipeline

#### Day 8-9: Data Normalizer Lambda

**File**: `src/data-normalizer/handler.py`

```python
import boto3
import csv
import json
from datetime import datetime
from io import StringIO

s3 = boto3.client('s3')

def lambda_handler(event, context):
    """Normalize CSV data: parse dates, convert currencies, sort by date."""
    
    bucket = 'processing-bucket'
    input_key = event['validated_key']
    
    # Download validated CSV
    response = s3.get_object(Bucket=bucket, Key=input_key)
    content = response['Body'].read().decode('utf-8')
    
    # Parse and normalize
    reader = csv.DictReader(StringIO(content))
    transactions = []
    
    for row in reader:
        transactions.append({
            'date': parse_date(row['Date']),
            'isin': row['ISIN'].strip().upper(),
            'type': row['Transaction Type'].upper(),  # BUY or SELL
            'quantity': float(row['Quantity']),
            'price': float(row['Price']),
            'currency': row['Currency'].upper(),
        })
    
    # Sort by date
    transactions.sort(key=lambda x: x['date'])
    
    # Write normalized JSON
    output_key = f"normalized/{event['job_id']}.json"
    s3.put_object(
        Bucket=bucket,
        Key=output_key,
        Body=json.dumps(transactions, indent=2),
    )
    
    return {
        **event,
        'normalized_key': output_key,
        'transaction_count': len(transactions),
    }

def parse_date(date_str):
    """Parse date in multiple formats (YYYY-MM-DD, DD/MM/YYYY, etc.)."""
    # Implementation
    return '2025-01-15'
```

**Deliverables**:
- [ ] Data Normalizer Lambda implemented (.NET)
- [ ] Date parsing for multiple formats
- [ ] Unit tests for edge cases (xUnit)
- [ ] Deployed and tested

---

#### Day 10-12: NBP Rate Fetcher Lambda

**File**: `src/nbp-rate-fetcher/handler.py`

```python
import boto3
import json
import requests
from concurrent.futures import ThreadPoolExecutor, as_completed

s3 = boto3.client('s3')

NBP_API_URL = 'https://api.nbp.pl/api/exchangerates/rates/a/{currency}/{date}/?format=json'

def lambda_handler(event, context):
    """Fetch NBP exchange rates for all unique (currency, date) pairs."""
    
    bucket = 'processing-bucket'
    input_key = event['normalized_key']
    
    # Download normalized transactions
    response = s3.get_object(Bucket=bucket, Key=input_key)
    transactions = json.loads(response['Body'].read())
    
    # Extract unique (currency, date) pairs (exclude PLN)
    rate_requests = set()
    for t in transactions:
        if t['currency'] != 'PLN':
            rate_requests.add((t['currency'], t['date']))
    
    # Fetch rates (sequential for Phase 1)
    rates = {}
    for currency, date in rate_requests:
        rate = fetch_nbp_rate(currency, date)
        rates[f"{currency}#{date}"] = rate
    
    # Write rates to S3
    output_key = f"rates/{event['job_id']}.json"
    s3.put_object(Bucket=bucket, Key=output_key, Body=json.dumps(rates, indent=2))
    
    return {
        **event,
        'rates_key': output_key,
        'unique_rates': len(rates),
    }

def fetch_nbp_rate(currency, date):
    """Fetch exchange rate from NBP API."""
    url = NBP_API_URL.format(currency=currency, date=date)
    
    try:
        response = requests.get(url, timeout=10)
        response.raise_for_status()
        data = response.json()
        return data['rates'][0]['mid']
    except requests.RequestException as e:
        # Log error, raise exception (Step Functions will retry)
        raise Exception(f"NBP API error for {currency} on {date}: {e}")
```

**Deliverables**:
- [ ] NBP Rate Fetcher Lambda implemented (.NET with HttpClient)
- [ ] Error handling and retries (Polly library)
- [ ] Unit tests with mocked NBP API (Moq)
- [ ] Integration test with live NBP API
- [ ] Deployed and tested

---

#### Day 13-14: Tax Calculator Lambda (FIFO Engine)

**File**: `src/tax-calculator/fifo_engine.py`

```python
from collections import deque
from typing import List, Dict

class FIFOEngine:
    """FIFO matching engine for capital gains calculation."""
    
    def __init__(self):
        self.holdings = {}  # {ISIN: deque of BUY transactions}
    
    def calculate(self, transactions: List[Dict], rates: Dict[str, float]) -> Dict:
        """Calculate capital gains using FIFO method."""
        
        gains = []
        losses = []
        
        for t in transactions:
            isin = t['isin']
            transaction_type = t['type']
            quantity = t['quantity']
            price_foreign = t['price']
            currency = t['currency']
            date = t['date']
            
            # Convert to PLN
            rate = rates.get(f"{currency}#{date}", 1.0) if currency != 'PLN' else 1.0
            price_pln = price_foreign * rate
            
            if transaction_type == 'BUY':
                # Add to holdings queue
                if isin not in self.holdings:
                    self.holdings[isin] = deque()
                self.holdings[isin].append({
                    'date': date,
                    'quantity': quantity,
                    'price_pln': price_pln,
                })
            
            elif transaction_type == 'SELL':
                # Match against earliest BUYs (FIFO)
                remaining = quantity
                sell_value_pln = price_pln * quantity
                
                while remaining > 0:
                    if not self.holdings[isin]:
                        raise Exception(f"Insufficient holdings for {isin} on {date}")
                    
                    buy = self.holdings[isin][0]
                    
                    if buy['quantity'] <= remaining:
                        # Full match
                        buy_value_pln = buy['price_pln'] * buy['quantity']
                        gain_loss = sell_value_pln - buy_value_pln
                        
                        if gain_loss > 0:
                            gains.append(gain_loss)
                        else:
                            losses.append(abs(gain_loss))
                        
                        remaining -= buy['quantity']
                        self.holdings[isin].popleft()
                    else:
                        # Partial match
                        buy_value_pln = buy['price_pln'] * remaining
                        gain_loss = sell_value_pln - buy_value_pln
                        
                        if gain_loss > 0:
                            gains.append(gain_loss)
                        else:
                            losses.append(abs(gain_loss))
                        
                        buy['quantity'] -= remaining
                        remaining = 0
        
        return {
            'total_gain': round(sum(gains), 2),
            'total_loss': round(sum(losses), 2),
            'net_gain': round(sum(gains) - sum(losses), 2),
            'gain_count': len(gains),
            'loss_count': len(losses),
        }
```

**File**: `src/tax-calculator/handler.py`

```python
import boto3
import json
from fifo_engine import FIFOEngine

s3 = boto3.client('s3')
dynamodb = boto3.resource('dynamodb')

def lambda_handler(event, context):
    """Calculate capital gains using FIFO method."""
    
    bucket = 'processing-bucket'
    
    # Download transactions and rates
    transactions = json.loads(s3.get_object(Bucket=bucket, Key=event['normalized_key'])['Body'].read())
    rates = json.loads(s3.get_object(Bucket=bucket, Key=event['rates_key'])['Body'].read())
    
    # Calculate
    engine = FIFOEngine()
    results = engine.calculate(transactions, rates)
    
    # Update DynamoDB job record
    jobs_table = dynamodb.Table('InvestTax-Jobs')
    jobs_table.update_item(
        Key={'job_id': event['job_id']},
        UpdateExpression='SET #s = :status, total_gain = :gain, total_loss = :loss',
        ExpressionAttributeNames={'#s': 'status'},
        ExpressionAttributeValues={
            ':status': 'CALCULATED',
            ':gain': results['total_gain'],
            ':loss': results['total_loss'],
        },
    )
    
    # Write results
    output_key = f"results/{event['job_id']}.json"
    s3.put_object(Bucket=bucket, Key=output_key, Body=json.dumps(results, indent=2))
    
    return {
        **event,
        'results_key': output_key,
        **results,
    }
```

**Testing**:
```python
# tests/test_fifo_engine.py
import pytest
from fifo_engine import FIFOEngine

def test_simple_gain():
    engine = FIFOEngine()
    transactions = [
        {'isin': 'US0378331005', 'type': 'BUY', 'date': '2025-01-01', 'quantity': 10, 'price': 100, 'currency': 'PLN'},
        {'isin': 'US0378331005', 'type': 'SELL', 'date': '2025-06-01', 'quantity': 10, 'price': 120, 'currency': 'PLN'},
    ]
    results = engine.calculate(transactions, {})
    assert results['total_gain'] == 200  # (120 - 100) * 10
    assert results['total_loss'] == 0
```

**Deliverables**:
- [ ] FIFO Engine implemented
- [ ] Tax Calculator Lambda implemented
- [ ] Comprehensive unit tests (80% coverage)
- [ ] Validated against reference calculator
- [ ] Deployed and tested

---

### Week 3: Output and Email Delivery

**Note**: Continue implementing all Lambda functions in .NET 8 following the patterns shown earlier. Use Amazon.Lambda.SimpleEmailEvents for SES integration.

#### Day 15-16: Report Generator Lambda

**Implementation Pattern** (C#):
```csharp
// src/InvestTax.ReportGenerator/Function.cs
// - Use RazorLight or string interpolation for text templates
// - For HTML emails (Phase 2): Use RazorLight with .cshtml templates
// - Write report to S3 using IAmazonS3.PutObjectAsync
```

**File**: `src/report-generator/handler.py`

```python
import boto3
import json
from datetime import datetime

s3 = boto3.client('s3')

def lambda_handler(event, context):
    """Generate plain text email report."""
    
    year = event['year']
    results = {
        'total_gain': event['total_gain'],
        'total_loss': event['total_loss'],
        'net_gain': event['net_gain'],
    }
    
    # Generate plain text report
    report = generate_text_report(year, results)
    
    # Write to S3
    bucket = 'processing-bucket'
    output_key = f"reports/{event['job_id']}.txt"
    s3.put_object(Bucket=bucket, Key=output_key, Body=report)
    
    return {
        **event,
        'report_key': output_key,
    }

def generate_text_report(year, results):
    """Generate plain text report."""
    return f"""
==============================================
   POLISH TAX CALCULATOR - PIT-38 SUMMARY
==============================================

Tax Year: {year}

CAPITAL GAINS SUMMARY:
- Total Capital Gains: {results['total_gain']:.2f} PLN
- Total Capital Losses: {results['total_loss']:.2f} PLN
- Net Gain/Loss: {results['net_gain']:.2f} PLN

TAX CALCULATION (19% flat rate):
- Tax Owed: {results['net_gain'] * 0.19:.2f} PLN

DISCLAIMER:
This report is for informational purposes only. Please consult
with a tax professional before filing your PIT-38 form.

Generated: {datetime.now().isoformat()}
==============================================
    """.strip()
```

**Deliverables**:
- [ ] Report Generator Lambda implemented
- [ ] Plain text template created
- [ ] Deployed and tested

---

#### Day 17-18: Email Sender Lambda

**File**: `src/email-sender/handler.py`

```python
import boto3

s3 = boto3.client('s3')
ses = boto3.client('ses', region_name='eu-central-1')

SENDER_EMAIL = 'noreply@investtax.example.com'

def lambda_handler(event, context):
    """Send tax report via email."""
    
    email = event['email']
    bucket = 'processing-bucket'
    report_key = event['report_key']
    
    # Download report
    response = s3.get_object(Bucket=bucket, Key=report_key)
    report_body = response['Body'].read().decode('utf-8')
    
    # Send email
    try:
        response = ses.send_email(
            Source=SENDER_EMAIL,
            Destination={'ToAddresses': [email]},
            Message={
                'Subject': {'Data': f'Your {event["year"]} Tax Report'},
                'Body': {'Text': {'Data': report_body}},
            },
        )
        
        return {
            **event,
            'email_status': 'SENT',
            'message_id': response['MessageId'],
        }
    
    except Exception as e:
        return {
            **event,
            'email_status': 'FAILED',
            'error': str(e),
        }
```

**AWS SES Setup**:
```bash
# Verify sender email
aws ses verify-email-identity --email-address noreply@investtax.example.com

# Check verification status
aws ses get-identity-verification-attributes --identities noreply@investtax.example.com

# Request sandbox exit (production only)
# https://console.aws.amazon.com/ses/ → Account dashboard → Request production access
```

**Deliverables**:
- [ ] Email Sender Lambda implemented
- [ ] SES sender email verified
- [ ] Error handling for bounces/rejections
- [ ] Deployed and tested

---

#### Day 19-21: Step Functions Workflow

**File**: `infrastructure/lib/stacks/workflow-stack.ts`

```typescript
import * as cdk from 'aws-cdk-lib';
import * as sfn from 'aws-cdk-lib/aws-stepfunctions';
import * as tasks from 'aws-cdk-lib/aws-stepfunctions-tasks';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import { Construct } from 'constructs';

export class WorkflowStack extends cdk.Stack {
  constructor(scope: Construct, id: string, lambdaFunctions: any, props?: cdk.StackProps) {
    super(scope, id, props);

    // Define tasks
    const extractMetadata = new tasks.LambdaInvoke(this, 'ExtractMetadata', {
      lambdaFunction: lambdaFunctions.metadataExtractor,
      outputPath: '$.Payload',
    });

    const validateCSV = new tasks.LambdaInvoke(this, 'ValidateCSV', {
      lambdaFunction: lambdaFunctions.csvValidator,
      outputPath: '$.Payload',
    });

    const normalizeData = new tasks.LambdaInvoke(this, 'NormalizeData', {
      lambdaFunction: lambdaFunctions.dataNormalizer,
      outputPath: '$.Payload',
    });

    const fetchNBPRates = new tasks.LambdaInvoke(this, 'FetchNBPRates', {
      lambdaFunction: lambdaFunctions.nbpRateFetcher,
      outputPath: '$.Payload',
    });

    const calculateTax = new tasks.LambdaInvoke(this, 'CalculateTax', {
      lambdaFunction: lambdaFunctions.taxCalculator,
      outputPath: '$.Payload',
    });

    const generateReport = new tasks.LambdaInvoke(this, 'GenerateReport', {
      lambdaFunction: lambdaFunctions.reportGenerator,
      outputPath: '$.Payload',
    });

    const sendEmail = new tasks.LambdaInvoke(this, 'SendEmail', {
      lambdaFunction: lambdaFunctions.emailSender,
      outputPath: '$.Payload',
    });

    // Error handling
    const sendErrorEmail = new tasks.LambdaInvoke(this, 'SendErrorEmail', {
      lambdaFunction: lambdaFunctions.emailSender,
      outputPath: '$.Payload',
    });

    // Define workflow
    const definition = extractMetadata
      .next(validateCSV)
      .next(
        new sfn.Choice(this, 'IsValid?')
          .when(sfn.Condition.stringEquals('$.status', 'VALID'), normalizeData)
          .otherwise(sendErrorEmail)
      )
      .next(fetchNBPRates)
      .next(calculateTax)
      .next(generateReport)
      .next(sendEmail);

    // Create state machine
    new sfn.StateMachine(this, 'TaxCalculationWorkflow', {
      definition,
      timeout: cdk.Duration.hours(1),
    });
  }
}
```

**Deploy**:
```bash
cdk deploy WorkflowStack
```

**Testing**:
```bash
# Trigger workflow manually
aws stepfunctions start-execution \
  --state-machine-arn arn:aws:states:eu-central-1:123456789012:stateMachine:TaxCalculationWorkflow \
  --input '{"bucket": "upload-bucket", "key": "test.csv", "email": "user@example.com"}'

# Check execution status
aws stepfunctions describe-execution --execution-arn <execution-arn>
```

**Deliverables**:
- [ ] Step Functions workflow created
- [ ] All Lambda functions integrated
- [ ] Error handling implemented
- [ ] End-to-end test successful
- [ ] **MILESTONE: Phase 1 MVP Complete**

---

## Phase 2: Production Enhancements (Weeks 4-12)

See [Phased Development](phased-development.md) for detailed Week 4-12 implementation plan covering:

- Week 4-5: Performance optimization (DynamoDB caching, parallel processing)
- Week 6: Monitoring and observability (X-Ray, CloudWatch dashboards)
- Week 7: Advanced error handling (SNS, DLQ, templates)
- Week 8: Data lifecycle management (S3 archival, lifecycle policies)
- Week 9: Enhanced UX (HTML email reports)
- Week 10: Security hardening (VPC, KMS)
- Week 11: CI/CD pipeline (GitHub Actions, canary deployment)
- Week 12: Final testing and documentation

---

## Success Criteria

### Phase 1 MVP

- [ ] User can upload CSV file to S3
- [ ] System processes file within 5 minutes
- [ ] User receives plain text email with tax summary
- [ ] FIFO calculation results match reference calculator (±0.01 PLN)
- [ ] Error rate < 5% for valid files
- [ ] All Lambda functions have basic error handling
- [ ] Code coverage > 60%

### Phase 2 Production

- [ ] Processing time < 3 minutes for 10K rows
- [ ] User receives HTML email report
- [ ] CloudWatch dashboard operational
- [ ] CloudWatch alarms configured
- [ ] X-Ray tracing enabled
- [ ] CI/CD pipeline functional
- [ ] Load test: 100 concurrent jobs successful
- [ ] Code coverage > 80%
- [ ] Documentation complete

---

## Deployment Checklist

### Development Environment

- [ ] CDK deployed to dev account
- [ ] Test CSV files uploaded
- [ ] Manual testing successful

### Staging Environment

- [ ] CDK deployed to staging account
- [ ] Integration tests passing
- [ ] Performance testing complete

### Production Environment

- [ ] SES production access approved
- [ ] Domain verified (SPF/DKIM configured)
- [ ] Budget alerts configured
- [ ] CloudWatch alarms enabled
- [ ] Runbooks documented
- [ ] User documentation published
- [ ] CDK deployed to production account
- [ ] Smoke tests passing
- [ ] Monitoring dashboards live

---

## Troubleshooting

### Common Issues

**1. Lambda Timeout**:
- **Symptom**: Function execution stops after 300s
- **Solution**: Increase timeout in CDK configuration (max 900s)

**2. NBP API Rate Limiting**:
- **Symptom**: HTTP 429 errors
- **Solution**: Implement exponential backoff, add jitter to retry delays

**3. SES Email Bounces**:
- **Symptom**: Email not delivered, bounce notification received
- **Solution**: Verify recipient email is valid, check spam folders

**4. DynamoDB Throttling**:
- **Symptom**: ProvisionedThroughputExceededException
- **Solution**: Ensure on-demand billing mode is enabled

**5. S3 Access Denied**:
- **Symptom**: Lambda cannot read/write S3 objects
- **Solution**: Verify IAM role has s3:GetObject and s3:PutObject permissions

**6. .NET Lambda Cold Start Latency**:
- **Symptom**: First invocation takes 500ms+
- **Solution**: Enable Native AOT compilation in .csproj (<PublishAot>true</PublishAot>) for <50ms cold starts

**7. NuGet Package Restore Failures**:
- **Symptom**: Build fails with missing package errors
- **Solution**: Run `dotnet restore` before `dotnet lambda package`, ensure packages.config includes all dependencies

---

## .NET Implementation Summary

This project uses **.NET 8** throughout:

- **Lambda Functions**: All 7 Lambda functions written in C# (.NET 8)
- **Infrastructure**: AWS CDK written in C# for consistency
- **Testing**: xUnit, FluentAssertions, and Moq for comprehensive test coverage
- **Deployment**: `dotnet lambda package` + AWS CDK deployment
- **Performance**: Native AOT compilation for <50ms cold starts
- **Benefits**: Strong typing, compile-time safety, excellent AWS SDK, high performance

---

## Next Steps

- **Review Architecture**: Refer to [Architecture Overview](overview.md) for high-level design
- **Understand Components**: See [Component Architecture](component-architecture.md) for detailed breakdown
- **Plan Monitoring**: Review [NFR Analysis](nfr-analysis.md) for observability requirements
- **Assess Risks**: Check [Risks & Technology Stack](risks-and-tech-stack.md) before starting

---

[← Back to Index](README.md) | [← Previous: Risks & Technology Stack](risks-and-tech-stack.md)

**Ready for Implementation! 🚀**
