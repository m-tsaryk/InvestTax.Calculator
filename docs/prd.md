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
  - Match sells to buys by ISIN using FIFO based on trade date.
  - Include transaction costs (commissions, fees) in cost basis calculation.
  - Compute taxable income and cost basis in PLN using NBP official exchange rates per trade date.
  - Apply Polish tax authority rounding rules (2 decimal places for PLN amounts).
  - Calculate total gains, losses, and net taxable amount for the reporting year.
  - Send an error summary email if calculation fails.

- **Human-readable summary** (Priority: P0)
  - Provide a final tax amount and a plain-language explanation of how it was calculated.
  - Include a breakdown by ISIN showing each matched buy-sell pair.
  - Show transaction costs included in calculations.
  - Display NBP exchange rates used for each transaction.
  - Clearly list assumptions (FIFO, exchange rate source, trade date usage, year boundary).
  - Include disclaimer that calculation is for informational purposes.

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
- Transactions outside the reporting year are excluded from calculation.
- Transaction costs (commissions, fees) included in gain/loss calculation.
- Missing NBP rate for a transaction date uses fallback logic.
- Corporate actions (splits, dividends) require manual exclusion from input file in MVP.
- Same-day buy and sell transactions handled correctly by FIFO ordering.

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
- National Bank of Poland (NBP) API for official PLN exchange rates by date.
- Fallback mechanism when NBP rate unavailable (send failure email).

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

## 9. Tax and accounting considerations

### 9.1 Cost basis calculation

- **Transaction costs inclusion**: Clarify whether brokerage commissions, fees, and custody charges should be added to cost basis and subtracted from sale proceeds per Polish tax law.
- **Acquisition cost adjustments**: Define how to handle any adjustments to purchase price (e.g., settlement fees, transfer taxes).
- **Rounding rules**: Implement Polish Tax Authority rounding rules (typically to 2 decimal places for PLN, but verify official guidance).

### 9.2 Currency conversion rules

- **Conversion date**: Specify whether to use trade date or settlement date for FX conversion (Polish regulations typically require trade date).
- **Official rates source**: Use National Bank of Poland (NBP) official exchange rates as the authoritative source for tax purposes.
- **Rate unavailability**: Define fallback logic when NBP rate is unavailable for a specific date (e.g., weekends, holidays) - stop processing and send failure email.
- **Mid-rate vs buy/sell rates**: Clarify which rate to use (NBP publishes average rates suitable for tax purposes).

### 9.3 Corporate actions handling

- **Stock splits and reverse splits**: Adjust cost basis and share quantity accordingly, maintaining total cost basis.
- **Dividends and distributions**: Exclude from capital gains calculation (reported separately on PIT-8C for dividend income).
- **Mergers and acquisitions**: Define how to handle stock conversions and cash-in-lieu payments.
- **Spin-offs and rights issues**: Allocate cost basis between original and new securities per tax rules.
- **Bonus shares**: Adjust cost basis per share when free shares are received.
- **Return of capital**: Reduce cost basis rather than treating as taxable income.

### 9.4 Transaction timing and settlement

- **Reporting year boundaries**: Use trade date (not settlement date) to determine which tax year a transaction belongs to.
- **Unsettled trades**: Clarify treatment of trades executed near year-end that settle in following year.
- **Cross-year positions**: Ensure buys from previous years can be matched to sells in reporting year.

### 9.5 Asset type considerations

- **Equity vs other instruments**: Confirm scope is limited to publicly traded stocks for MVP.
- **ETFs and funds**: Clarify if treated same as individual stocks or require different handling.
- **Fractional shares**: Define rounding and matching rules for partial share quantities.
- **Derivatives exclusion**: Explicitly exclude options, futures, and other derivatives from MVP scope.

### 9.6 Loss handling and limitations

- **Loss offset rules**: Implement rules for offsetting gains with losses within the same tax year.
- **Loss carryforward**: Document whether unused losses can be carried to future years (not implemented in MVP but should be noted).
- **Wash sale considerations**: Research if Poland has wash sale rules (buying back same security within specific period).

### 9.7 Documentation and audit trail

- **Calculation transparency**: Provide detailed breakdown showing each matched buy-sell pair with dates, quantities, prices, FX rates, and resulting gain/loss.
- **Source data retention**: Maintain original CSV and all intermediate calculation steps for audit purposes.
- **Assumption documentation**: Clearly state all assumptions, limitations, and edge cases in the summary report.
- **Disclaimer**: Include statement that the calculation is for informational purposes and users should verify with tax professional.

### 9.8 Compliance and accuracy validation

- **Reference calculations**: Test against known scenarios and manual calculations to verify accuracy.
- **Edge case testing**: Validate handling of same-day buys and sells, zero-cost basis, very small quantities.
- **Regulatory updates**: Plan for annual review of tax rules and calculation methodology.
- **Professional review**: Consider having tax accountant review calculation logic before production release.

### 9.9 Reporting requirements for PIT-38

- **Form sections mapping**: Identify which PIT-38 sections/lines should be populated with calculated values.
- **Supporting documentation**: List what additional documents users should attach to PIT-38 (transaction confirmations, broker statements).
- **Income classification**: Confirm capital gains are reported in correct income category per Polish tax code.
- **Tax rate application**: Verify current capital gains tax rate (19% for Poland as of recent years, but confirm for reporting year).

### 9.10 Special scenarios and exclusions

- **Tax-exempt accounts**: Clarify that calculator assumes taxable accounts (not IKE/IKZE retirement accounts).
- **Non-resident considerations**: Note that calculator is for Polish tax residents only.
- **First sale of inherited shares**: Exclude from scope if special rules apply.
- **Employee stock options/RSUs**: Exclude from MVP as they have different cost basis rules.

## 10. Milestones & sequencing

### 10.1 Project estimate

- Small: 3-5 weeks

### 10.2 Team size & composition

- 3 people: backend engineer, frontend engineer, QA/analyst

### 10.3 Suggested phases

- **Phase 1**: File intake, validation, and calculation engine (2-3 weeks)
  - Key deliverables: FIFO matching, PIT-38 totals in PLN, validation errors.
- **Phase 2**: Summary report and email delivery (1-2 weeks)
  - Key deliverables: Human-readable summary, email sending, retention policy.

## 11. Final checklist

- All user stories are testable with objective acceptance criteria.
- Acceptance criteria are specific and measurable.
- Functional scope covers upload, validation, calculation, and email delivery.
- Security and privacy requirements are explicitly defined.