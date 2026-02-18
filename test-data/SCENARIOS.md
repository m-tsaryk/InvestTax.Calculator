# InvestTax Calculator - Test Scenarios Documentation

**Version**: 1.0  
**Last Updated**: February 19, 2026  
**Purpose**: Comprehensive documentation of all integration test scenarios

---

## Table of Contents

1. [Overview](#overview)
2. [Test Scenario Structure](#test-scenario-structure)
3. [Success Scenarios](#success-scenarios)
4. [Error Scenarios](#error-scenarios)
5. [Edge Cases and Future Tests](#edge-cases-and-future-tests)

---

## Overview

This document provides detailed documentation of all integration test scenarios for the InvestTax Calculator MVP. Each scenario is designed to test specific functionality, including:

- **Happy path workflows** (successful tax calculation)
- **Error handling** (validation failures, API errors)
- **FIFO algorithm correctness** (partial fills, multiple stocks)
- **Data normalization** (date formats, currencies)
- **External API integration** (NBP exchange rates)
- **Email notifications** (success and error cases)

---

## Test Scenario Structure

Each test scenario includes:

- **Scenario ID**: Unique identifier
- **Test Name**: Descriptive name
- **Purpose**: What the test validates
- **Input Data**: CSV file content and metadata
- **Expected Output**: Anticipated results
- **Edge Cases Covered**: Specific scenarios tested
- **Validation Points**: How to verify success
- **Performance Expectations**: Execution time targets

---

## Success Scenarios

### Scenario 1: Simple Buy-Sell Transaction

**Test File**: `test-1-simple-success.csv`  
**Expected File**: `test-1-simple-success.expected.json`  
**Purpose**: Validate basic end-to-end workflow with a single buy and sell transaction

#### Input Data

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|2024-01-15 10:30:00|US0378331005|AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|Initial purchase
Sell|2024-06-20 14:45:00|US0378331005|AAPL|Apple Inc.|10|180.00|USD|4.10|1800.00|7380.00|Sale for profit
```

#### Transaction Details

| Field | Buy Transaction | Sell Transaction |
|-------|----------------|------------------|
| Action | Buy | Sell |
| Date | 2024-01-15 10:30:00 | 2024-06-20 14:45:00 |
| ISIN | US0378331005 | US0378331005 |
| Ticker | AAPL | AAPL |
| Quantity | 10 shares | 10 shares |
| Price/Share | $150.00 USD | $180.00 USD |
| Total USD | $1,500.00 | $1,800.00 |
| Exchange Rate | 3.95 PLN/USD | 4.10 PLN/USD |
| Total PLN | 5,925.00 PLN | 7,380.00 PLN |

#### Expected Calculation

**FIFO Matching**:
- Sell 10 shares (2024-06-20) matched with Buy 10 shares (2024-01-15)

**Capital Gain Calculation**:
```
Cost Basis (PLN)    = 10 × $150 × 3.95 = 5,925.00 PLN
Sale Proceeds (PLN) = 10 × $180 × 4.10 = 7,380.00 PLN
Capital Gain (PLN)  = 7,380.00 - 5,925.00 = 1,455.00 PLN
Tax (19%)           = 1,455.00 × 0.19 = 276.45 PLN
```

#### Expected Output

```json
{
  "totalGainPLN": 1455.00,
  "totalTaxPLN": 276.45,
  "matchedTransactions": 1,
  "transactionDetails": [
    {
      "isin": "US0378331005",
      "ticker": "AAPL",
      "buyDate": "2024-01-15",
      "sellDate": "2024-06-20",
      "quantity": 10,
      "buyPriceUSD": 150.00,
      "sellPriceUSD": 180.00,
      "gainUSD": 300.00,
      "buyExchangeRate": 3.95,
      "sellExchangeRate": 4.10,
      "gainPLN": 1455.00,
      "taxPLN": 276.45
    }
  ]
}
```

#### Edge Cases Covered

- ✅ Simple FIFO matching (1:1 buy to sell)
- ✅ Positive capital gain scenario
- ✅ Single currency (USD)
- ✅ Single stock (AAPL)
- ✅ NBP API integration for 2 dates
- ✅ Plain text report generation
- ✅ Success email delivery

#### Validation Points

- [ ] CSV parsed successfully without errors
- [ ] ISIN validated (US0378331005 is valid format)
- [ ] Dates parsed correctly (ISO 8601 format)
- [ ] NBP rates fetched for 2024-01-15 and 2024-06-20
- [ ] FIFO matching applied correctly
- [ ] PLN calculations accurate to 2 decimal places
- [ ] Tax calculated at exactly 19%
- [ ] Report generated with correct values
- [ ] Success email sent to specified recipient
- [ ] DynamoDB job status: `Completed`

#### Performance Expectations

- Total execution time: **< 2 minutes**
- Breakdown:
  - Validation: < 10 seconds
  - Normalization: < 10 seconds
  - NBP Rate Fetch: < 30 seconds (2 API calls)
  - Tax Calculation: < 5 seconds
  - Report Generation: < 5 seconds
  - Email Sending: < 5 seconds

---

### Scenario 2: Multiple Stocks with Various Transactions

**Test File**: `test-2-multiple-stocks.csv`  
**Expected File**: `test-2-multiple-stocks.expected.json`  
**Purpose**: Validate FIFO algorithm across multiple stocks (ISINs) with partial sells

#### Input Data

**Summary**:
- 3 different stocks: AAPL, MSFT, GOOGL
- 8 total transactions (4 buys, 4 sells)
- Partial AAPL position (not fully sold)

#### Transaction Breakdown

**AAPL (US0378331005)**:
1. Buy 15 shares @ $145.50 (2024-01-10)
2. Sell 10 shares @ $165.00 (2024-04-25) → Partial sell
3. Buy 5 shares @ $155.00 (2024-05-10)
4. Sell 10 shares @ $175.00 (2024-07-20) → Uses remainder from first buy + second buy

**MSFT (US5949181045)**:
1. Buy 8 shares @ $380.00 (2024-02-15)
2. Sell 8 shares @ $410.00 (2024-06-15) → Complete sell

**GOOGL (IE00B4L5Y983)**:
1. Buy 5 shares @ $140.00 (2024-03-20)
2. Sell 5 shares @ $155.00 (2024-08-30) → Complete sell

#### Expected FIFO Matching

**AAPL Matching**:
- First sell (10 shares) matched with first buy (10 out of 15)
- Second sell (10 shares) matched with:
  - Remaining 5 shares from first buy
  - First 5 shares from second buy
- Remaining position: 0 shares (all sold)

**MSFT Matching**:
- Sell 8 shares matched with buy 8 shares (1:1)

**GOOGL Matching**:
- Sell 5 shares matched with buy 5 shares (1:1)

#### Expected Output

```json
{
  "totalGainPLN": "calculated at runtime",
  "totalTaxPLN": "calculated at runtime",
  "matchedTransactions": 4,
  "notes": "Tests FIFO across multiple stocks, partial sells, and multiple buys"
}
```

**Manual Calculation Required**: Expected gains per stock need to be calculated based on actual NBP rates at transaction dates.

#### Edge Cases Covered

- ✅ Multiple ISINs processed independently
- ✅ FIFO applied separately per ISIN
- ✅ Partial sells (AAPL first sell only uses 10 of 15 shares)
- ✅ Multiple buys for same stock (AAPL)
- ✅ Complete liquidation of some positions (MSFT, GOOGL)
- ✅ Mixed transaction ordering (interleaved buys/sells across stocks)
- ✅ Different currencies (all USD but tests normalization)
- ✅ NBP API calls for multiple dates

#### Validation Points

- [ ] All 8 transactions parsed successfully
- [ ] 3 distinct ISINs recognized
- [ ] FIFO applied independently for each ISIN
- [ ] AAPL partial sell handled correctly (10 of 15)
- [ ] AAPL second sell correctly uses remaining 5 + new 5
- [ ] MSFT and GOOGL fully matched
- [ ] Report shows breakdown by ISIN
- [ ] Total gain is sum of gains from all 3 stocks
- [ ] Tax calculated on total gain
- [ ] Success email includes all stock details

#### Performance Expectations

- Total execution time: **< 4 minutes**
- NBP API calls: ~8 requests (4 buy dates + 4 sell dates)
- Larger CSV processing overhead

---

### Scenario 3: Complex Partial Fills (FIFO Stress Test)

**Test File**: `test-3-partial-fills.csv`  
**Expected File**: `test-3-partial-fills.expected.json`  
**Purpose**: Validate complex FIFO scenarios where sells partially consume multiple buy lots

#### Input Data

**TSLA (US88160R1014) Transactions**:
1. Buy 20 shares @ $180.00 (2024-01-05)
2. Buy 10 shares @ $175.00 (2024-02-10)
3. Sell 15 shares @ $195.00 (2024-03-15)
4. Sell 8 shares @ $200.00 (2024-04-20)
5. Buy 12 shares @ $185.00 (2024-05-25)
6. Sell 10 shares @ $210.00 (2024-06-30)

#### Expected FIFO Matching

**First Sell (15 shares @ $195, 2024-03-15)**:
- Consumes 15 shares from first buy (20 shares @ $180, 2024-01-05)
- Remaining from first buy: 5 shares

**Second Sell (8 shares @ $200, 2024-04-20)**:
- Consumes remaining 5 shares from first buy
- Consumes 3 shares from second buy (10 shares @ $175, 2024-02-10)
- Remaining from second buy: 7 shares

**Third Sell (10 shares @ $210, 2024-06-30)**:
- Consumes remaining 7 shares from second buy
- Consumes 3 shares from third buy (12 shares @ $185, 2024-05-25)
- Remaining from third buy: 9 shares

**Final Positions**:
- Total bought: 42 shares
- Total sold: 33 shares
- **Remaining unsold: 9 shares @ $185 average**

#### Expected Output

```json
{
  "totalGainPLN": "calculated at runtime",
  "totalTaxPLN": "calculated at runtime",
  "matchedTransactions": 3,
  "remainingPositions": [
    {
      "isin": "US88160R1014",
      "ticker": "TSLA",
      "quantity": 9,
      "averageCostUSD": 185.00,
      "buyDate": "2024-05-25"
    }
  ],
  "notes": "Tests complex FIFO scenario where sells partially consume multiple buy lots"
}
```

#### Edge Cases Covered

- ✅ Sells consuming multiple buy lots
- ✅ Partial consumption of buy lots
- ✅ Remaining unsold positions tracked
- ✅ Sequential partial fills
- ✅ Chronological buy/sell ordering
- ✅ FIFO queue management across 6 transactions

#### Validation Points

- [ ] First sell correctly consumes 15 of 20 from first buy
- [ ] Second sell spans two buy lots (5 + 3)
- [ ] Third sell spans two buy lots (7 + 3)
- [ ] Remaining position calculated: 9 shares
- [ ] Report shows all matched transactions with partial indicators
- [ ] Report shows remaining positions section
- [ ] Total gain is sum of all 3 sell transactions
- [ ] Tax calculated only on realized gains (not unsold positions)

#### Performance Expectations

- Total execution time: **< 3 minutes**
- NBP API calls: ~6 requests

---

## Error Scenarios

### Scenario 4: Validation Error - Missing ISIN

**Test File**: `test-4-validation-error-missing-isin.csv`  
**Expected File**: `test-4-validation-error-missing-isin.expected.json`  
**Purpose**: Validate that missing required fields are caught and reported

#### Input Data

Row 2 has empty ISIN field:

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|2024-01-15 10:30:00|US0378331005|AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|Valid transaction
Buy|2024-02-20 11:00:00||MSFT|Microsoft Corp.|5|380.00|USD|3.98|1900.00|7562.00|Missing ISIN - should fail
Sell|2024-06-20 14:45:00|US0378331005|AAPL|Apple Inc.|10|180.00|USD|4.10|1800.00|7380.00|Valid transaction
```

#### Expected Validation Errors

```json
{
  "errors": [
    {
      "row": 2,
      "column": "ISIN",
      "value": "",
      "message": "ISIN is required and cannot be empty",
      "validationRule": "NotEmpty"
    }
  ]
}
```

#### Expected Workflow

1. ✅ File uploaded to S3
2. ✅ Starter Lambda triggered
3. ✅ Validator Lambda invoked
4. ❌ **Validation fails** on row 2
5. ✅ Error response returned to Step Functions
6. ✅ Step Functions routes to error handler
7. ✅ Error email sent with validation details
8. ✅ Job status updated to `Failed` in DynamoDB
9. 🛑 Workflow stops (no normalization, calculation, etc.)

#### Expected Output

```json
{
  "jobStatus": "Failed",
  "errorType": "ValidationError",
  "validationErrors": [
    {
      "row": 2,
      "column": "ISIN",
      "message": "ISIN is required"
    }
  ],
  "errorEmailSent": true
}
```

#### Edge Cases Covered

- ✅ Required field validation (ISIN)
- ✅ Error detection before processing
- ✅ Graceful error handling in workflow
- ✅ Error email notification
- ✅ Proper job status tracking

#### Validation Points

- [ ] Validator Lambda detects missing ISIN
- [ ] Validation error includes row number and column name
- [ ] Step Functions transitions to error handler state
- [ ] Error email sent with clear instructions
- [ ] Email includes:
  - Job ID
  - Error type
  - Specific row and column
  - Actionable guidance (how to fix)
- [ ] DynamoDB job status: `Failed`
- [ ] No subsequent Lambdas invoked (Normalizer, NBP, etc.)

#### Performance Expectations

- Total execution time: **< 1 minute**
- Fast failure (no expensive operations after validation)

---

### Scenario 5: Validation Error - Invalid Date Format

**Test File**: `test-5-validation-error-invalid-date.csv`  
**Expected File**: `test-5-validation-error-invalid-date.expected.json`  
**Purpose**: Validate date format validation and error reporting

#### Input Data

Row 2 has invalid date format (month 13):

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|2024-01-15 10:30:00|US0378331005|AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|Valid transaction
Buy|2024-13-45 11:00:00|US5949181045|MSFT|Microsoft Corp.|5|380.00|USD|3.98|1900.00|7562.00|Invalid date format - should fail
Sell|2024-06-20 14:45:00|US0378331005|AAPL|Apple Inc.|10|180.00|USD|4.10|1800.00|7380.00|Valid transaction
```

#### Expected Validation Errors

```json
{
  "errors": [
    {
      "row": 2,
      "column": "Time",
      "value": "2024-13-45 11:00:00",
      "message": "Invalid date format. Expected: yyyy-MM-dd HH:mm:ss",
      "validationRule": "DateFormat"
    }
  ]
}
```

#### Expected Output

```json
{
  "jobStatus": "Failed",
  "errorType": "ValidationError",
  "validationErrors": [
    {
      "row": 2,
      "column": "Time",
      "message": "Invalid date format"
    }
  ],
  "errorEmailSent": true
}
```

#### Edge Cases Covered

- ✅ Date format validation
- ✅ Invalid month detection (13)
- ✅ Invalid day detection (45)
- ✅ Error reporting with specific value
- ✅ Mixed valid/invalid rows

#### Validation Points

- [ ] Date parsing error detected
- [ ] Row 2 identified as problematic
- [ ] Error message includes expected format
- [ ] Error email sent with date format guidance
- [ ] DynamoDB job status: `Failed`
- [ ] Workflow stopped before normalization

#### Performance Expectations

- Total execution time: **< 1 minute**

---

## Edge Cases and Future Tests

### Phase 2 Test Scenarios (Not Yet Implemented)

#### Test 6: Capital Loss Scenario

**Purpose**: Validate handling of negative gains (losses)  
**Input**: Buy high, sell low transactions  
**Expected**: Loss tracked but no tax owed (or loss carry-forward)

---

#### Test 7: Multi-Currency Transactions

**Purpose**: Test transactions in EUR, GBP, CHF in addition to USD  
**Input**: Mix of currency symbols  
**Expected**: Correct NBP rates fetched for all currencies

---

#### Test 8: Weekend/Holiday NBP Rate Handling

**Purpose**: Validate fallback when NBP API returns 404 for weekends  
**Input**: Transactions on Saturday/Sunday  
**Expected**: Previous business day rate used

---

#### Test 9: Large File Performance Test

**Purpose**: Test performance with 1,000+ transactions  
**Input**: Generated CSV with 1,000 rows  
**Expected**: Completion in < 10 minutes

---

#### Test 10: Concurrent Upload Stress Test

**Purpose**: Test multiple simultaneous uploads  
**Input**: 10 uploads within 1 minute  
**Expected**: All processed successfully, no race conditions

---

#### Test 11: Same-Day Buy and Sell

**Purpose**: Validate intraday transactions  
**Input**: Buy and sell on same date  
**Expected**: Same exchange rate used for both transactions

---

#### Test 12: Multiple Tax Years

**Purpose**: Test transactions spanning multiple tax years  
**Input**: Buys in 2023, sells in 2024  
**Expected**: Only 2024 sells reported for 2024 tax year

---

#### Test 13: Corporate Actions (Future)

**Purpose**: Handle stock splits, dividends (future enhancement)  
**Input**: Stock split notation in Notes field  
**Expected**: Split-adjusted cost basis

---

### Known Limitations (MVP Phase)

- ❌ No HTML report generation (plain text only)
- ❌ No NBP rate caching (fetches every time)
- ❌ No parallel processing (sequential Lambda invocations)
- ❌ No loss carry-forward tracking
- ❌ No multi-year tax reporting
- ❌ No corporate action handling (splits, dividends)
- ❌ No transaction editing/correction workflow
- ❌ No authentication/user management

---

## Test Maintenance Guidelines

### Adding New Test Scenarios

1. Create CSV file: `test-data/test-N-description.csv`
2. Create expected output: `test-data/test-N-description.expected.json`
3. Add test case to `run-integration-tests.ps1`
4. Document scenario in this file (SCENARIOS.md)
5. Update TEST-CHECKLIST.md with validation steps
6. Run test and verify results
7. Commit test files to version control

### Updating Existing Scenarios

1. Update CSV file with new data
2. Recalculate expected outputs
3. Update expected JSON file
4. Update documentation in this file
5. Re-run test to verify
6. Update TEST-CHECKLIST.md if validation changes

---

## Appendix: Test Data Generation

### Generating Test Data Programmatically

For future test cases, consider using a script to generate test data:

```powershell
# Example: Generate test data for large file test
function Generate-LargeTestFile {
    param($Rows = 1000)
    
    $csv = "Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes`n"
    
    $stocks = @(
        @{ISIN="US0378331005"; Ticker="AAPL"; Name="Apple Inc."},
        @{ISIN="US5949181045"; Ticker="MSFT"; Name="Microsoft Corp."},
        @{ISIN="IE00B4L5Y983"; Ticker="GOOGL"; Name="Alphabet Inc."}
    )
    
    for ($i = 0; $i -lt $Rows; $i++) {
        $stock = $stocks[$i % $stocks.Count]
        $action = if ($i % 2 -eq 0) { "Buy" } else { "Sell" }
        $date = (Get-Date "2024-01-01").AddDays($i).ToString("yyyy-MM-dd HH:mm:ss")
        $shares = Get-Random -Minimum 1 -Maximum 100
        $price = [math]::Round((Get-Random -Minimum 100 -Maximum 500) + (Get-Random) / 100, 2)
        $rate = [math]::Round((Get-Random -Minimum 3.5 -Maximum 4.5), 2)
        $total = [math]::Round($shares * $price, 2)
        $totalPLN = [math]::Round($total * $rate, 2)
        
        $csv += "$action|$date|$($stock.ISIN)|$($stock.Ticker)|$($stock.Name)|$shares|$price|USD|$rate|$total|$totalPLN|Generated test data`n"
    }
    
    $csv | Out-File "test-data/test-large-file.csv" -Encoding UTF8
}
```

---

**End of Test Scenarios Documentation**
