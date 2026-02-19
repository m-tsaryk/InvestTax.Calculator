# Architecture Overview

[← Back to Index](README.md)

## Executive Summary

The InvestTax Calculator is a serverless, event-driven AWS solution designed to help Polish retail investors calculate their annual capital gains taxes for PIT-38 filings. The system leverages AWS managed services to provide a scalable, reliable, and cost-effective solution that processes transaction CSV files, calculates tax obligations using FIFO matching methodology, and delivers detailed reports via email.

### Key Architectural Principles

- **Serverless-first**: Utilizing AWS Lambda and managed services to minimize operational overhead
- **Event-driven**: Asynchronous processing triggered by S3 upload events
- **Security-focused**: Encryption at rest and in transit, least-privilege IAM policies
- **Cost-optimized**: Pay-per-use model with no idle resource costs
- **Scalability**: Auto-scaling Lambda functions to handle variable workloads
- **Reliability**: Built-in retries, dead-letter queues, and comprehensive error handling

### High-Level Flow

1. User uploads CSV file to S3 bucket with email metadata
2. S3 event triggers Step Functions workflow
3. Parallel validation, normalization, and NBP rate fetching
4. FIFO-based tax calculation engine processes transactions
5. Human-readable report generation
6. Email delivery via SES
7. Automatic cleanup after retention period

---

## Architecture Approach

The InvestTax Calculator follows a **serverless, event-driven microservices architecture** built entirely on AWS managed services. This approach provides:

### Event-Driven Processing
S3 upload events trigger AWS Step Functions state machine orchestrating the entire workflow. This eliminates the need for polling, reduces latency, and provides natural decoupling between components.

### Decoupled Components
Each processing stage (validation, normalization, calculation, reporting) runs as independent Lambda functions. This allows:
- Independent scaling per component
- Isolated failures (one stage failure doesn't crash the entire system)
- Easier testing and deployment
- Clear separation of concerns

### Asynchronous Execution
No synchronous API calls; all processing happens asynchronously post-upload. Users don't wait for processing to complete—they receive results via email when ready.

### Managed Services
Leveraging SQS, SNS, DynamoDB, SES, and EventBridge for reliability and scalability:
- **No server management**: AWS handles patching, scaling, availability
- **Built-in redundancy**: Multi-AZ deployment automatic
- **Pay-per-use**: Zero costs when idle

### Stateless Compute
Lambda functions maintain no state; all state stored in S3 and DynamoDB:
- Functions can be safely retried or scaled
- No session affinity required
- Simplified error recovery

---

## Core Architectural Patterns

### 1. Choreography over Orchestration (Hybrid)
**Pattern**: Step Functions orchestrates high-level workflow while individual components communicate via events.

**Why**: 
- Central visibility into workflow status
- Built-in retry and error handling
- Visual workflow representation for debugging
- Event-based communication between stages maintains loose coupling

**Trade-off**: Slightly higher cost than pure event-driven (SQS/SNS), but significantly better observability.

---

### 2. Strangler Fig for NBP Integration
**Pattern**: Abstraction layer over NBP API allows future rate source substitution.

**Why**:
- NBP API is external dependency outside our control
- Future requirements may need multiple rate sources
- Can swap implementation without changing calling code

**Implementation**: NBP Client library encapsulates all API interactions.

---

### 3. Saga Pattern
**Pattern**: Compensating transactions for cleanup on failure.

**Why**:
- Distributed transactions across S3, DynamoDB, SES
- Need to clean up resources if workflow fails midway
- Maintain data consistency

**Implementation**: Step Functions error handlers trigger cleanup lambda (delete temp files, mark job as failed).

---

### 4. Circuit Breaker
**Pattern**: NBP API calls include retry logic with exponential backoff.

**Why**:
- External API may be temporarily unavailable
- Avoid overwhelming failing service
- Fail fast when service is down

**Implementation**:
- 3 retry attempts with 1s, 2s, 4s delays
- After 3 failures, circuit opens for 30 seconds
- Error email sent to user with clear explanation

---

### 5. Bulkhead
**Pattern**: Separate Lambda functions isolate failures between processing stages.

**Why**:
- Failure in validation doesn't impact tax calculation
- Resource limits (memory, timeout) tailored per stage
- Independent monitoring and alerting

**Implementation**: 7 separate Lambda functions, each with dedicated IAM role and resource allocation.

---

## System Characteristics

### Scalability
- **Horizontal**: Unlimited concurrent file uploads (Lambda auto-scales)
- **Vertical**: Individual files up to 100K rows (Lambda 15-min limit)
- **Bottleneck**: NBP API rate limits (mitigated by caching)

### Performance
- **Target**: < 5 minutes for 100K row file
- **Actual** (Phase 2): 2-3 minutes for 10K rows with cache hits
- **Optimization**: DynamoDB caching, parallel NBP API calls

### Reliability
- **Availability**: 99.9% target (based on AWS SLA composition)
- **Fault Tolerance**: Automatic retries at every level (Lambda, Step Functions, SDK)
- **Data Durability**: S3 99.999999999%, DynamoDB point-in-time recovery

### Security
- **Encryption**: At rest (SSE-S3/KMS) and in transit (TLS 1.2+)
- **Access Control**: IAM least-privilege per component
- **Compliance**: GDPR-ready (EU region, retention policies, encryption)

### Cost
- **Model**: Pay-per-use, zero idle costs
- **Estimated**: $100-500/month for 100-1000 calculations
- **Optimization**: S3 lifecycle to Glacier, DynamoDB on-demand, right-sized Lambda memory

---

## Technology Stack Summary

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Compute** | AWS Lambda (Python 3.12) | Serverless processing |
| **Orchestration** | AWS Step Functions | Workflow management |
| **Storage** | Amazon S3 | File storage (upload, temp, archive) |
| **Database** | Amazon DynamoDB | Job tracking & rate caching |
| **Email** | Amazon SES | Report delivery |
| **Events** | Amazon EventBridge | Scheduling & event routing |
| **Monitoring** | CloudWatch + X-Ray | Logging, metrics, tracing |
| **Security** | IAM + KMS + Secrets Manager | Access control & encryption |
| **IaC** | AWS CDK (TypeScript/Python) | Infrastructure as Code |

---

## Workflow Summary

```
User Upload → S3 Event → Step Functions
  ↓
  1. Extract Metadata (create job record)
  ↓
  2. Validate CSV (structure, data types)
  ↓
  3. Normalize Data (dates, currencies, grouping)
  ↓
  4. Fetch NBP Rates (with caching)
  ↓
  5. Calculate Tax (FIFO matching, PLN conversion)
  ↓
  6. Generate Report (HTML + plain text)
  ↓
  7. Send Email (via SES)
  ↓
  8. Archive & Cleanup
```

**Error Handling**: At any stage, validation/processing failures trigger error email to user with specific guidance.

---

## Next Steps

- **Deep Dive**: Review [Component Architecture](component-architecture.md) for detailed layer breakdown
- **Infrastructure**: See [Deployment Architecture](deployment-architecture.md) for AWS setup
- **Processing**: Explore [Data Flow](data-flow.md) for stage-by-stage processing details
- **Implementation**: Check [Phased Development](phased-development.md) for MVP and production timeline

---

[← Back to Index](README.md) | [Next: System Context →](system-context.md)
