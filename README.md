# InvestTax.Calculator

**Automated Capital Gains Tax Calculator for Polish Investors**

---

## 📋 Project Overview

InvestTax.Calculator is a serverless, event-driven solution designed to simplify the annual capital gains tax calculation process for Polish retail investors filing PIT-38 forms. The system automates the complex task of matching buy and sell transactions, converting foreign currencies using official NBP exchange rates, and calculating taxable income according to Polish tax authority regulations.

### The Problem

Individual investors trading international stocks face significant challenges when filing their annual PIT-38 tax returns:

- **Manual spreadsheet work**: Tedious tracking of hundreds of buy/sell transactions
- **FIFO matching complexity**: Determining which purchase corresponds to which sale
- **Currency conversion**: Converting USD/EUR transactions to PLN using official NBP rates
- **Transaction costs**: Properly accounting for commissions and fees in cost basis
- **Calculation errors**: Risk of mistakes with complex multi-currency, multi-stock portfolios
- **Time-consuming process**: Hours or days spent preparing tax documentation

### The Solution

InvestTax.Calculator provides a fully automated workflow:

1. **Simple Upload**: User uploads a CSV file containing their annual transaction history
2. **Automatic Processing**: System validates data, fetches exchange rates, and performs calculations
3. **Transparent Results**: User receives a detailed email report with step-by-step calculation breakdown
4. **PIT-38 Ready**: Report provides all necessary information for completing the tax filing

---

## 🏗️ AWS Resources Used

### Compute & Orchestration
- **AWS Lambda** (7 functions):
  - `Starter`: Initiates workflow from S3 events
  - `Validator`: Validates CSV structure and data integrity
  - `Normalizer`: Normalizes dates, currencies, and transaction data
  - `NBPClient`: Fetches exchange rates from NBP API
  - `Calculator`: Performs FIFO matching and tax calculations
  - `ReportGenerator`: Creates human-readable reports
  - `EmailSender`: Delivers reports via email

- **AWS Step Functions**: Orchestrates the entire workflow with error handling and retries

### Storage & Data
- **Amazon S3**:
  - `input-bucket`: Receives uploaded CSV files
  - `processed-bucket`: Stores normalized data and generated reports
  - `archive-bucket`: Long-term storage with lifecycle policies

- **Amazon DynamoDB**:
  - `ExchangeRates`: Caches NBP exchange rates (30-day TTL, 90% hit rate)
  - `JobStatus`: Tracks processing status for each upload

### Communication
- **Amazon SES**: Sends email reports to users
- **Amazon EventBridge**: Routes S3 upload events to Step Functions
- **Amazon SQS**: Dead-letter queues for failed operations

### Monitoring & Security
- **AWS CloudWatch**: Logging, metrics, and alarms
- **AWS CloudTrail**: Audit logging for compliance
- **AWS IAM**: Least-privilege access control
- **AWS KMS**: Encryption key management

### External Services
- **NBP API** (api.nbp.pl): Official exchange rates from the National Bank of Poland

---

## 🎯 Core Features

### Input Processing
- Accepts pipe-delimited CSV files with transaction data (buys, sells, commissions)
- Validates data integrity (required fields, date formats, numeric values)
- Normalizes dates, currencies, and transaction details

### Tax Calculation Engine
- **FIFO Matching**: Matches sell orders to buy orders using First-In-First-Out methodology based on trade dates
- **Multi-Currency Support**: Handles USD, EUR, GBP with automatic PLN conversion
- **Official Exchange Rates**: Uses NBP (National Bank of Poland) published rates for each transaction date
- **Transaction Costs**: Includes broker commissions and fees in cost basis calculations
- **Polish Tax Rules**: Applies PIT-38 rounding rules (2 decimal places for PLN amounts)

### Reporting
- Human-readable summary with plain-language explanations
- Transaction-by-transaction breakdown showing each matched buy-sell pair
- Exchange rates used for each conversion
- Total taxable gains, losses, and net amount
- Clear statement of assumptions (FIFO, NBP rates, trade date usage)
- Legal disclaimer for informational purposes

### Error Handling
- Comprehensive validation with actionable error messages
- Automatic retries for transient failures
- Dead-letter queues for failed operations
- Detailed error emails when processing fails

---

## 💡 Real-World Example

### Scenario
Anna is a Polish investor who traded US stocks through Interactive Brokers in 2025. She needs to file her PIT-38 form and calculate capital gains tax.

### Her Transaction History
```csv
Action|Time|ISIN|Ticker|Name/Notes|ID|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Currency (Result)|Total|Currency (Total)
Market buy|2025-01-15 10:30:00|US0378331005|AAPL|Apple Inc.|TXN001|10|150.00|USD|3.9234||USD|-1500.00|USD
Market sell|2025-06-20 14:45:00|US0378331005|AAPL|Apple Inc.|TXN002|-10|175.00|USD|3.8721||USD|1750.00|USD
Commission|2025-01-15 10:30:00|US0378331005|AAPL|Commission||||||USD|-1.50|USD
Commission|2025-06-20 14:45:00|US0378331005|AAPL|Commission||||||USD|-1.50|USD
```

### Processing Flow

1. **Upload**: Anna uploads her CSV to the S3 input bucket with metadata `email=anna@example.com`

2. **Automatic Processing**:
   - **Validation**: System confirms all required fields are present, dates are valid, ISIN codes are correct
   - **NBP Rate Lookup**: Fetches official exchange rates:
     - 2025-01-15: 1 USD = 3.9234 PLN
     - 2025-06-20: 1 USD = 3.8721 PLN
   - **FIFO Matching**: Matches the June sell to the January buy
   - **Tax Calculation**:
     ```
     Cost Basis (PLN) = (10 shares × $150 + $1.50) × 3.9234 = 5,891.40 PLN
     Sale Proceeds (PLN) = (10 shares × $175 - $1.50) × 3.8721 = 6,771.13 PLN
     Taxable Gain = 6,771.13 - 5,891.40 = 879.73 PLN
     ```

3. **Email Delivery**: Anna receives a detailed report:
   ```
   Subject: Your PIT-38 Tax Calculation Summary

   Total Taxable Income: 879.73 PLN

   Transaction Details:
   ISIN: US0378331005 (Apple Inc.)
   - Buy: 2025-01-15, 10 shares @ $150.00 (3.9234 PLN/USD) = 5,891.40 PLN
   - Sell: 2025-06-20, 10 shares @ $175.00 (3.8721 PLN/USD) = 6,771.13 PLN
   - Gain: 879.73 PLN

   Exchange Rates Source: NBP (Official Polish National Bank rates)
   Calculation Method: FIFO (First-In-First-Out)
   
   Disclaimer: This calculation is for informational purposes only...
   ```

### Result
Anna has all the information she needs to complete her PIT-38 form, with transparent calculations showing exactly how the 879.73 PLN taxable gain was derived.

---

## 📊 System Architecture

The system follows a **serverless, event-driven microservices architecture**:

```
User Upload → S3 → EventBridge → Step Functions → Lambda Functions → Email Report
                                        ↓
                                   DynamoDB Cache ← NBP API
```

### Key Architectural Principles

- **Serverless-first**: Zero server management, automatic scaling
- **Event-driven**: Asynchronous processing triggered by S3 uploads
- **Security-focused**: Encryption at rest and in transit, least-privilege IAM
- **Cost-optimized**: Pay-per-use model with no idle resource costs
- **Reliability**: Built-in retries, dead-letter queues, comprehensive error handling

---

## 📁 Repository Structure

```
├── docs/                          # Architecture and planning documentation
│   ├── architecture/              # Detailed architecture diagrams and analysis
│   ├── prd.md                     # Product Requirements Document
│   └── user-stories.md            # User stories and acceptance criteria
├── infrastructure/                # Infrastructure as Code
│   └── cdk/                       # AWS CDK stack definitions (TypeScript)
├── scripts/                       # Deployment and testing scripts
├── src/                           # C# .NET application code
│   ├── InvestTax.Core/           # Domain models, validators, interfaces
│   ├── InvestTax.Infrastructure/ # AWS SDK wrappers, utilities
│   └── InvestTax.Lambda.*/       # Individual Lambda function implementations
├── test-data/                     # Sample CSV files and test scenarios
└── tests/                         # Unit and integration tests
```

---

## 🔒 Security & Compliance

- **Data Encryption**: All data encrypted at rest (S3 SSE-KMS) and in transit (TLS 1.2+)
- **Access Control**: IAM roles with least-privilege permissions
- **Audit Logging**: CloudTrail captures all API calls for 90 days
- **Data Retention**: Configurable retention policies (default 30 days)
- **GDPR Compliance**: Designed with privacy regulations in mind

---

## 📈 Scalability & Performance

- **Auto-scaling**: Lambda functions scale automatically from 0 to 3000 concurrent executions
- **Cache Optimization**: 90% NBP rate cache hit rate reduces API calls
- **Parallel Processing**: Multiple transactions processed simultaneously
- **Performance Target**: < 2 minutes for typical portfolios (< 1000 transactions)

---

## 🎓 Technology Stack

- **Backend**: C# .NET 8.0 (Lambda runtime)
- **Infrastructure**: AWS CDK (TypeScript)
- **Cloud Platform**: AWS (Lambda, S3, DynamoDB, Step Functions, SES)
- **Data Format**: CSV (pipe-delimited)
- **External API**: NBP API for official exchange rates

---

## 📄 License

This project is intended for educational and personal use. Users are responsible for verifying all tax calculations with qualified tax professionals before filing official tax documents.

---

## ⚠️ Disclaimer

**This tool is for informational purposes only and does not constitute tax advice.** 

The calculations provided are based on publicly available information about Polish tax regulations and FIFO capital gains methodologies. However:

- Tax laws are complex and subject to change
- Individual circumstances may require different treatment
- Results should be verified by a qualified tax professional
- The creators of this tool assume no liability for the accuracy of calculations
- Users are solely responsible for the accuracy of their tax filings

Always consult with a certified tax advisor or accountant before filing official tax documents.

---

*For technical documentation, setup instructions, and deployment guides, please refer to the `/docs` directory.*