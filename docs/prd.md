# PRD: InvestTax Calculator

## 1. Product overview

### 1.1 Document title and version

- PRD: InvestTax Calculator
- Version: 0.1.0

### 1.2 Product summary

This project provides a simple way for individual investors in Poland to calculate annual capital gains taxes for PIT-38 based on their transaction history. Users upload a delimited CSV file that contains their buy and sell orders for the year.

The system validates and parses the input, matches buys to sells, converts values using provided exchange rates, and computes taxable gains and losses according to Polish PIT-38 rules. It then produces a clear, human-readable summary explaining the calculation and sends it to the user by email.

The MVP focuses on a single-country rule set (Poland), no broker integrations, and a limited input format to keep the experience straightforward and reliable.

## 2. Goals

### 2.1 Business goals

- Deliver a trustworthy PIT-38 tax calculation workflow for Polish retail investors.
- Reduce manual spreadsheet work by automating matching, conversion, and totals.
- Provide a clear email report that users can confidently use for filing.

### 2.2 User goals

- Upload a transaction history file quickly and reliably.
- Receive a transparent summary of taxes owed with supporting calculations.
- Understand how each transaction affected the final result.

### 2.3 Non-goals

- Multi-country tax rules in the MVP.
- Direct integrations with brokers or trading platforms.
- Filing PIT-38 directly with government systems.

## 3. User personas

### 3.1 Key user types

- Retail investor in Poland with annual buy and sell orders.

### 3.2 Basic persona details

- **Retail investor**: Manages personal investments and wants a simple, accurate tax summary for PIT-38.

### 3.3 Role-based access

- **Investor**: Uploads a file, triggers calculation, receives the summary email.

## 4. Functional requirements

- **CSV intake and validation** (Priority: P0)
  - Accept a pipe-delimited file with header columns:
    - Action|Time|ISIN|Ticker|Name/Notes|ID|No. of shares|Price / share|Currency (Price / share)|Exchange rate|Result|Currency (Result)|Total|Currency (Total)
  - Validate required columns, data types, and missing values.
  - Reject files with unsupported delimiters or missing required fields, and return a clear error.
  - Send an error summary email if validation fails.

- **Transaction normalization** (Priority: P0)
  - Normalize dates to a single timezone and format.
  - Normalize numeric fields and currencies using the public FX rate API.
  - Group transactions by ISIN and sort by time.

- **Tax calculation (Poland PIT-38)** (Priority: P0)
  - Match sells to buys by ISIN using FIFO.
  - Compute taxable income and cost basis in PLN using public FX rates per transaction date.
  - Calculate total gains, losses, and net taxable amount for the reporting year.
  - Send an error summary email if calculation fails.

- **Human-readable summary** (Priority: P0)
  - Provide a final tax amount and a plain-language explanation of how it was calculated.
  - Include a breakdown by ISIN and transaction-level totals.
  - Clearly list assumptions (FIFO, exchange rate usage, year boundary).

- **Email delivery** (Priority: P0)
  - Send the summary to the specified email address.
  - Provide a confirmation that the email was sent or an error if delivery failed.
  - Send failure emails with actionable guidance when validation or calculation errors occur.

- **File storage** (Priority: P1)
  - Accept files from object storage (e.g., AWS S3) for processing.
  - Support a configurable retention policy for uploaded files and generated reports.

## 5. User experience

### 5.1 Entry points & first-time user flow

- User uploads or provides a link to the CSV file.
- User provides the email address for the summary.
- System validates the file and starts processing.

### 5.2 Core experience

- **Upload and validation**: User submits the file and gets immediate feedback on format issues.
  - Ensures the user can fix errors before waiting for processing.
- **Calculation**: System matches buys to sells and computes PIT-38 totals.
  - Ensures accuracy and transparency of the tax calculation.
- **Summary delivery**: User receives an email with the final result and explanation.
  - Ensures the user has a permanent record for filing.
- **Failure handling**: User receives an email describing what failed and how to fix it.
  - Ensures users can recover without guessing.

### 5.3 Advanced features & edge cases

- Partial fills and multiple buys matched to a single sell.
- Multiple currencies in a single file with per-row currencies.
- Transactions outside the reporting year are excluded.
- Missing FX rate for a transaction date triggers a failure email.

### 5.4 UI/UX highlights

- Clear error messages that point to the offending row and column.
- Summary text that is readable by non-experts and auditable by power users.

## 6. Narrative

An investor uploads their annual order history, the system validates it, applies FIFO matching and exchange-rate conversion, and calculates their PIT-38 totals. The investor receives a clear email with the final tax amount and a transparent explanation they can trust for filing.

## 7. Success metrics

### 7.1 User-centric metrics

- Time from upload to summary email under 5 minutes for typical files.
- Less than 5 percent of uploads result in validation errors after documentation is read.

### 7.2 Business metrics

- 100 percent of users successfully complete an annual calculation on first attempt.

### 7.3 Technical metrics

- 99 percent of processed files complete without system errors.
- Calculation results match a reference calculator within 0.01 PLN.

## 8. Technical considerations

### 8.1 Integration points

- Object storage for input files (e.g., AWS S3).
- Email service for summary delivery.
- Public FX rate API for PLN conversion by date.

### 8.2 Data storage & privacy

- Store original input and generated summary for a configurable retention period (default 30 days).
- Encrypt stored files at rest and in transit.

### 8.3 Scalability & performance

- Process files up to 100,000 rows within 5 minutes.
- Support concurrent processing without cross-user data exposure.

### 8.4 Potential challenges

- Edge cases with corporate actions or malformed data.
- FX API availability or missing historical rates.
- User confusion around exchange rates or reporting year boundaries.

## 9. Milestones & sequencing

### 9.1 Project estimate

- Small: 3-5 weeks3

### 9.2 Team size & composition

- 3 people: backend engineer, frontend engineer, QA/analyst

### 9.3 Suggested phases

- **Phase 1**: File intake, validation, and calculation engine (2-3 weeks)
  - Key deliverables: FIFO matching, PIT-38 totals in PLN, validation errors.
- **Phase 2**: Summary report and email delivery (1-2 weeks)
  - Key deliverables: Human-readable summary, email sending, retention policy.

## 10. Final checklist

- All user stories are testable with objective acceptance criteria.
- Acceptance criteria are specific and measurable.
- Functional scope covers upload, validation, calculation, and email delivery.
- Security and privacy requirements are explicitly defined.