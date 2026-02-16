# Risks and Technology Stack

[← Back to Index](README.md)

## Overview

This document identifies potential risks to the InvestTax Calculator system and provides the detailed technology stack recommendations with rationale for each choice.

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation | Residual Risk |
|------|-----------|--------|-----------|--------------|
| **NBP API Unavailability** | Medium | High | - DynamoDB caching (90% hit rate)<br/>- Retry logic with exponential backoff<br/>- User notification with clear guidance | Low |
| **Incorrect Tax Calculation** | Low | Critical | - Reference calculator validation<br/>- Comprehensive unit tests (80% coverage)<br/>- Tax professional review of logic<br/>- Clear disclaimer in reports | Low |
| **Large File Timeout** (> 100K rows) | Medium | Medium | - Lambda timeout: 900s<br/>- Future: Implement chunking<br/>- Document file size limits | Medium |
| **Email Delivery Failure** | Low | Medium | - SES retry logic (5 attempts)<br/>- Dead Letter Queue for investigation<br/>- CloudWatch alarm for operations team | Low |
| **Data Privacy Breach** | Very Low | Critical | - Encryption at rest (SSE-S3, KMS)<br/>- Encryption in transit (TLS 1.2+)<br/>- IAM least privilege<br/>- Regular security audits | Very Low |
| **Cost Overrun** | Low | Low | - CloudWatch budget alerts<br/>- On-demand pricing (pay-per-use)<br/>- S3 lifecycle policies (Glacier)<br/>- Monthly cost review | Very Low |
| **Dependency Vulnerabilities** | Medium | Medium | - Automated dependency scanning (pip-audit)<br/>- Monthly updates<br/>- Security patch process | Low |
| **Regional AWS Outage** | Very Low | High | - Cross-region replication (archive bucket)<br/>- Manual failover runbook<br/>- 4-hour RTO documented | Low |
| **NBP Exchange Rate Changes** | Very Low | Medium | - Cache immutability (rates never change once published)<br/>- Historical data validation | Very Low |
| **Compliance Violation** (GDPR) | Low | High | - Data retention policies (30 days default)<br/>- Encryption and access controls<br/>- Regular compliance audits | Very Low |

---

### Risk Details

#### 1. NBP API Unavailability

**Description**: The NBP API (api.nbp.pl) may become unavailable due to maintenance, outages, or rate limiting.

**Impact**: Without exchange rates, tax calculations cannot be completed for international transactions.

**Mitigations**:
- **DynamoDB Caching**: 90% of requests hit cache (30-day TTL)
- **Retry Logic**: 3 attempts with exponential backoff (1s, 2s, 4s)
- **Circuit Breaker**: After 5 consecutive failures, circuit opens for 30 seconds
- **User Communication**: Clear error message explaining delay and expected resolution

**Residual Risk**: Low - Cache covers most scenarios; temporary NBP outages rarely impact users.

---

#### 2. Incorrect Tax Calculation

**Description**: FIFO matching algorithm or exchange rate logic contains bugs, leading to incorrect tax amounts.

**Impact**: Users file incorrect PIT-38 forms, potential legal consequences.

**Mitigations**:
- **Reference Calculator**: Results validated against manual calculations
- **Unit Tests**: 80% code coverage, edge cases tested (same-day buy/sell, partial matches, multiple currencies)
- **Tax Professional Review**: Algorithm reviewed by certified tax advisor
- **Disclaimer**: Email report includes clear disclaimer that results are informational only

**Residual Risk**: Low - Multiple validation layers reduce risk, but users should verify critical calculations.

---

#### 3. Large File Timeout

**Description**: Files with > 100K rows may exceed Lambda's 15-minute execution limit.

**Impact**: Job fails, user does not receive results.

**Mitigations**:
- **Extended Timeout**: Lambda timeout set to 900s (15 minutes)
- **Future Enhancement**: Implement chunking (split by ISIN, process in parallel)
- **Documentation**: Clear file size limits in user guide

**Residual Risk**: Medium - Phase 1 supports up to 100K rows; larger files require Phase 3 enhancements.

---

#### 4. Email Delivery Failure

**Description**: Amazon SES may reject email (invalid address, bounces, spam filters).

**Impact**: User does not receive tax report.

**Mitigations**:
- **Retry Logic**: 5 retry attempts
- **Dead Letter Queue**: Failed emails captured for manual investigation
- **CloudWatch Alarm**: Triggers when email failure rate > 1%
- **User Notification**: If email fails after retries, job status in DynamoDB marked as "EMAIL_FAILED"

**Residual Risk**: Low - SES has 99.9% deliverability; operations team can manually resend.

---

#### 5. Data Privacy Breach

**Description**: Unauthorized access to user transaction data or personal information.

**Impact**: GDPR violation, reputational damage, potential fines (up to 4% of revenue or €20M).

**Mitigations**:
- **Encryption at Rest**: S3 SSE-S3 (Phase 1), KMS CMK (Phase 2)
- **Encryption in Transit**: HTTPS/TLS 1.2+
- **IAM Least Privilege**: Each Lambda has minimal permissions
- **Audit Logging**: CloudTrail logs all API calls for 90 days
- **Regular Audits**: Quarterly security reviews

**Residual Risk**: Very Low - Multiple security layers make breach highly unlikely.

---

#### 6. Cost Overrun

**Description**: Unexpected increase in AWS costs due to high usage or misconfiguration.

**Impact**: Budget exceeded, potential service suspension.

**Mitigations**:
- **Budget Alerts**: CloudWatch Budgets set at $500/month threshold
- **On-Demand Pricing**: DynamoDB scales automatically, only pay for usage
- **S3 Lifecycle Policies**: Transition to Glacier after 90 days (70% cost reduction)
- **Monthly Reviews**: Operations team reviews Cost Explorer reports

**Residual Risk**: Very Low - Serverless pay-per-use model limits cost surprises.

---

#### 7. Dependency Vulnerabilities

**Description**: Python dependencies (e.g., `requests`, `pandas`) contain security vulnerabilities.

**Impact**: Potential for code injection, data leaks, or denial-of-service attacks.

**Mitigations**:
- **Automated Scanning**: `pip-audit` runs on every deployment
- **Monthly Updates**: Dependency updates tested in staging monthly
- **Security Patch Process**: Critical CVEs patched within 48 hours
- **Lambda Layers**: Shared dependencies scanned once, reused across functions

**Residual Risk**: Low - Proactive scanning and updates minimize vulnerability window.

---

#### 8. Regional AWS Outage

**Description**: Entire AWS eu-central-1 region becomes unavailable (rare but possible).

**Impact**: System completely unavailable until region recovers.

**Mitigations**:
- **Cross-Region Replication**: S3 Archive bucket replicated to eu-west-1
- **Failover Runbook**: Documented procedure to deploy to backup region (4-hour RTO)
- **DynamoDB Global Tables** (Phase 2+): Jobs table replicated to backup region
- **Communication Plan**: Notify users via website status page

**Residual Risk**: Low - AWS region outages are extremely rare (last major outage: 2017).

---

#### 9. NBP Exchange Rate Changes

**Description**: NBP publishes incorrect or revised exchange rates (historical data changes).

**Impact**: Cached rates become stale, calculations incorrect.

**Mitigations**:
- **Cache Immutability**: NBP historical rates never change (confirmed by NBP API documentation)
- **Validation**: Compare fetched rates against known published rates during testing
- **TTL**: 30-day cache TTL allows for re-validation if needed

**Residual Risk**: Very Low - NBP historical rates are immutable by design.

---

#### 10. GDPR Compliance Violation

**Description**: Failure to properly handle personal data (email, transaction history) per GDPR requirements.

**Impact**: Legal action, fines up to €20M or 4% of global revenue.

**Mitigations**:
- **Data Minimization**: Only collect email and transaction data (no names, addresses)
- **Storage Limitation**: Default 30-day retention, configurable deletion
- **Right to Erasure**: Manual deletion process documented in runbook
- **Data Residency**: EU region (eu-central-1) for EU users
- **Encryption**: At rest and in transit
- **Consent**: User consent implied by upload action (documented in Terms of Service)

**Residual Risk**: Very Low - Architecture designed for GDPR compliance from day one.

---

## Technology Stack Recommendations

### Compute

**AWS Lambda (.NET 8)**

- **Rationale**:
  - Serverless - no infrastructure management
  - Auto-scaling - handles variable load automatically
  - Pay-per-execution - cost-efficient for event-driven workload
  - Strong typing and performance - C# provides compile-time safety and excellent performance
  - Mature AWS SDK - comprehensive .NET SDK for all AWS services
  - Native AOT compilation - <50ms cold starts with ahead-of-time compilation

- **Alternatives Considered**:
  - **AWS Fargate**: More complex setup, overkill for event-driven workload
  - **EC2**: Requires instance management, not cost-effective for variable load
  - **Python on Lambda**: Slower performance, dynamic typing leads to runtime errors

- **Trade-offs**:
  - ✅ Pros: Zero management, auto-scaling, cost-efficient, strong typing, excellent performance
  - ❌ Cons: 15-minute execution limit, cold starts mitigated with Native AOT (<50ms)

---

### Orchestration

**AWS Step Functions (Standard Workflows)**

- **Rationale**:
  - Visual workflow - easy to understand and debug
  - Built-in retry logic - automatic error handling
  - State persistence - can resume after failures
  - Audit trail - every execution logged
  - Native integration with Lambda, DynamoDB, SES

- **Alternatives Considered**:
  - **Amazon SQS + SNS**: Requires more custom code for orchestration, manual retry logic, no visual workflow
  - **AWS Batch**: Designed for batch computing, not event-driven orchestration

- **Trade-offs**:
  - ✅ Pros: Visual design, state persistence, error handling
  - ❌ Cons: Higher cost ($25 per million state transitions vs. SQS $0.40 per million messages)
  - **Decision**: Visual workflow and built-in retry justify the cost for operational simplicity

---

### Storage

**Amazon S3**

- **Rationale**:
  - Unlimited storage capacity
  - 99.999999999% (11 nines) durability
  - Lifecycle policies - automatic archival to Glacier
  - Event notifications - trigger workflows on upload
  - Versioning - can restore previous versions
  - Cost-effective - $0.023/GB/month

- **Alternatives Considered**:
  - **Amazon EFS**: More expensive, designed for shared file systems (not needed)
  - **DynamoDB**: Not designed for large binary objects (CSV files)

- **Trade-offs**:
  - ✅ Pros: Unlimited storage, event notifications, lifecycle management
  - ❌ Cons: None - clear best fit

---

### Database

**Amazon DynamoDB (On-Demand Billing)**

- **Rationale**:
  - **Jobs Table**: Track job metadata (status, email, totals)
    - Millisecond latency for status lookups
    - Auto-scaling with on-demand billing
    - TTL support - automatic cleanup after 30 days
  - **Rates Cache**: Store NBP exchange rates
    - Key-value store (currency#date → rate)
    - 80-95% cache hit rate
    - TTL support - auto-expire after 30 days

- **Alternatives Considered**:
  - **RDS PostgreSQL**: Requires instance management, overkill for simple key-value storage, higher cost (~$50/month minimum)
  - **ElastiCache Redis**: More expensive, requires VPC configuration, not needed for this use case

- **Trade-offs**:
  - ✅ Pros: Serverless, auto-scaling, millisecond latency, TTL support
  - ❌ Cons: Limited query capabilities (no complex SQL queries)
  - **Decision**: Simple key-value access patterns fit DynamoDB perfectly

**Purpose of Database**:

1. **Jobs Table**:
   - **Primary Use**: Track job processing status (PENDING → PROCESSING → SUCCESS/FAILED)
   - **Data Stored**: job_id, email, upload_time, file_key, year, transaction_count, total_gain, total_loss, status
   - **Access Patterns**:
     - Write job metadata on upload (Step Functions)
     - Update status at each stage (Lambda functions)
     - Read final status for user notifications
   - **Why Database?**: Need real-time status tracking, fast lookups, automatic cleanup (TTL)

2. **Rates Cache Table**:
   - **Primary Use**: Cache NBP exchange rates to reduce API calls
   - **Data Stored**: currency#date (partition key), rate, fetched_at, TTL
   - **Access Patterns**:
     - Read rate by currency+date (NBP Fetcher Lambda)
     - Write rate if cache miss
     - Auto-delete after 30 days (TTL)
   - **Why Database?**: Sub-10ms latency for cache lookups, 80-95% hit rate, automatic expiration

**Why Not Just Use S3?**
- **S3**: Best for storing large files (CSVs, reports), not for fast lookups
- **DynamoDB**: Optimized for key-value queries (milliseconds vs. seconds)
- **Combination**: S3 stores files, DynamoDB stores metadata and cache

---

### Email Delivery

**Amazon SES**

- **Rationale**:
  - Native AWS integration with Lambda and Step Functions
  - Pay-per-email - $0.10 per 1,000 emails
  - High deliverability - SPF/DKIM support
  - Template support - HTML and plain text emails
  - Bounce/complaint handling - automatic feedback

- **Alternatives Considered**:
  - **SendGrid**: Third-party service, similar cost, additional dependency
  - **Custom SMTP Server**: Complex setup, deliverability challenges

- **Trade-offs**:
  - ✅ Pros: Native integration, cost-effective, high deliverability
  - ❌ Cons: Requires one-time sandbox exit (manual AWS support request)

---

### Monitoring

**Amazon CloudWatch + AWS X-Ray**

- **Rationale**:
  - **CloudWatch**: Native logging, metrics, alarms, dashboards
  - **X-Ray**: Distributed tracing, service maps, bottleneck identification
  - Tight integration with Lambda, Step Functions, DynamoDB
  - Pay-per-use pricing

- **Alternatives Considered**:
  - **Datadog**: Third-party, richer features, higher cost ($15/host/month), additional dependency
  - **ELK Stack (Elasticsearch, Logstash, Kibana)**: Self-managed, complex setup, infrastructure overhead

- **Trade-offs**:
  - ✅ Pros: Native integration, sufficient features for needs
  - ❌ Cons: Less powerful visualization than Datadog
  - **Decision**: CloudWatch/X-Ray provide 90% of features at 10% of cost

---

### Infrastructure as Code

**AWS CDK (C#)**

- **Rationale**:
  - Type-safe infrastructure definitions
  - Reusable constructs (DRY principle)
  - Generates CloudFormation templates
  - Same language as Lambda functions (C#) - consistency across codebase
  - Best practices built-in (e.g., encryption by default)

- **Alternatives Considered**:
  - **Terraform**: Multi-cloud support (not needed - AWS-only), HCL syntax less familiar to .NET developers
  - **CloudFormation (YAML)**: Verbose, harder to maintain, no type safety
  - **CDK TypeScript**: Different language from Lambda functions, context switching overhead

- **Trade-offs**:
  - ✅ Pros: Type safety, reusability, developer-friendly
  - ❌ Cons: Requires CDK learning curve
  - **Decision**: Higher productivity after initial learning

---

### External API Client

**HttpClient (.NET)**

- **Rationale**:
  - Industry-standard HTTP client for .NET
  - Simple API for GET/POST requests
  - Built-in retry logic with Polly library
  - Async/await support for non-blocking I/O
  - IHttpClientFactory for proper connection management

- **Alternatives Considered**:
  - **RestSharp**: Third-party library, adds unnecessary dependency
  - **Custom HTTP library**: Reinventing the wheel

- **Trade-offs**:
  - ✅ Pros: Native to .NET, reliable, modern async patterns
  - ❌ Cons: None

---

### Testing Frameworks

**xUnit (.NET), FluentAssertions**

- **Rationale**:
  - Industry-standard testing framework for .NET
  - Rich assertion library with FluentAssertions
  - Easy to write and maintain tests
  - Integrated with CI/CD pipelines
  - Excellent async/await test support

- **Alternatives Considered**:
  - **NUnit**: Older framework, less modern syntax
  - **MSTest**: Microsoft's framework, less popular in community

- **Trade-offs**:
  - ✅ Pros: De facto standard for .NET, great developer experience
  - ❌ Cons: None

---

### CI/CD

**GitHub Actions**

- **Rationale**:
  - Native GitHub integration
  - Generous free tier (2,000 minutes/month)
  - YAML-based configuration
  - Rich marketplace for actions (AWS deploy, test runners)

- **Alternatives Considered**:
  - **AWS CodePipeline**: More expensive, less flexible
  - **Jenkins**: Self-managed, complex setup

- **Trade-offs**:
  - ✅ Pros: Free, integrated with GitHub, easy to configure
  - ❌ Cons: Runner performance can vary
  - **Decision**: Best fit for GitHub-hosted repository

---

## Technology Stack Summary

| Layer | Technology | Cost (Monthly) | Justification |
|-------|-----------|----------------|---------------|
| **Compute** | AWS Lambda (.NET 8) | $10-50 | Serverless, auto-scaling, strong typing, Native AOT |
| **Orchestration** | AWS Step Functions | $5-15 | Visual workflow, built-in retry, state persistence |
| **Storage** | Amazon S3 | $5-20 | Unlimited capacity, event notifications, lifecycle policies |
| **Database** | Amazon DynamoDB | $5-10 | Millisecond latency, auto-scaling, TTL support |
| **Email** | Amazon SES | $1-5 | Native integration, high deliverability |
| **Monitoring** | CloudWatch + X-Ray | $5-10 | Native logging, distributed tracing |
| **IaC** | AWS CDK (C#) | $0 | Type-safe, reusable constructs, same language as Lambda |
| **CI/CD** | GitHub Actions | $0 | GitHub integration, free tier |
| **Total** | | **$31-120** | Scales with usage |

---

## Next Steps

- **Architecture Review**: Confirm technology choices with stakeholders
- **Implementation Planning**: Detail Phase 1 tasks in [Implementation Guide](implementation-guide.md)
- **Risk Assessment**: Schedule quarterly risk reviews

---

[← Back to Index](README.md) | [← Previous: NFR Analysis](nfr-analysis.md) | [Next: Implementation Guide →](implementation-guide.md)
