# Test Data Directory

This directory contains test CSV files for testing the InvestTax Calculator workflow.

---

## CSV Format

The CSV files should follow the Trading 212 export format with the following columns:

```
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
```

### Column Descriptions

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| **Action** | String | Transaction type (`Buy` or `Sell`) | `Buy` |
| **Time** | DateTime | Transaction timestamp | `2024-01-15 10:30:00` |
| **ISIN** | String | International Securities ID | `US0378331005` |
| **Ticker** | String | Stock ticker symbol | `AAPL` |
| **Name** | String | Company name | `Apple Inc.` |
| **No. of shares** | Decimal | Number of shares | `10` |
| **Price / share** | Decimal | Price per share in original currency | `150.00` |
| **Currency symbol** | String | Currency code | `USD` |
| **Exchange rate** | Decimal | Exchange rate to PLN | `3.95` |
| **Result** | Decimal | Total in original currency | `1500.00` |
| **Total** | Decimal | Total in PLN | `5925.00` |
| **Notes** | String | Optional transaction notes | `Initial purchase` |

---

## Sample Files

### sample.csv

Basic example with one buy and one sell transaction:
- 2 transactions (1 buy, 1 sell)
- Single stock (AAPL)
- Simple FIFO matching
- Expected profit: ~1,455 PLN

---

## Creating Test Files

### Test Case 1: Simple Success (2 transactions)

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|2024-01-15 10:30:00|US0378331005|AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|Initial purchase
Sell|2024-06-20 14:45:00|US0378331005|AAPL|Apple Inc.|10|180.00|USD|4.10|1800.00|7380.00|Sale for profit
```

### Test Case 2: Multiple Stocks

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|2024-01-15 10:30:00|US0378331005|AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|
Sell|2024-06-20 14:45:00|US0378331005|AAPL|Apple Inc.|5|180.00|USD|4.10|900.00|3690.00|
Buy|2024-02-10 09:15:00|US5949181045|MSFT|Microsoft Corp.|20|380.00|USD|3.98|7600.00|30248.00|
Sell|2024-07-15 16:20:00|US5949181045|MSFT|Microsoft Corp.|20|420.00|USD|4.05|8400.00|34020.00|
```

### Test Case 3: Partial FIFO Fills

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|2024-01-10 10:00:00|US0378331005|AAPL|Apple Inc.|100|145.00|USD|3.92|14500.00|56840.00|First batch
Buy|2024-02-15 11:30:00|US0378331005|AAPL|Apple Inc.|50|155.00|USD|3.96|7750.00|30690.00|Second batch
Sell|2024-06-20 14:45:00|US0378331005|AAPL|Apple Inc.|120|180.00|USD|4.10|21600.00|88560.00|Partial match
```

### Test Case 4: Validation Error - Missing ISIN

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|2024-01-15 10:30:00||AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|Missing ISIN
```

### Test Case 5: Validation Error - Invalid Date

```csv
Action|Time|ISIN|Ticker|Name|No. of shares|Price / share|Currency symbol|Exchange rate|Result|Total|Notes
Buy|invalid-date|US0378331005|AAPL|Apple Inc.|10|150.00|USD|3.95|1500.00|5925.00|Invalid date format
```

---

## Using Test Data

### Upload with Test Script

```powershell
# Upload sample.csv
.\scripts\test-upload.ps1 `
    -BucketName "investtax-upload-dev" `
    -Email "test@example.com" `
    -CsvFile "test-data\sample.csv"

# Upload custom test file
.\scripts\test-upload.ps1 `
    -BucketName "investtax-upload-dev" `
    -Email "john.doe@example.com" `
    -CsvFile "test-data\my-test.csv"
```

### Manual Upload

```powershell
# Using AWS CLI
aws s3 cp test-data/sample.csv s3://investtax-upload-dev/test@example.com/sample.csv --region eu-central-1
```

---

## Expected Outcomes

### Successful Processing

For valid CSV files:
1. File uploaded to S3
2. Starter Lambda triggered
3. Job record created in DynamoDB
4. Step Functions workflow started
5. CSV validated
6. Data normalized
7. Exchange rates fetched (if needed)
8. Tax calculated
9. Report generated
10. Email sent with results

### Validation Errors

For invalid CSV files:
1. File uploaded to S3
2. Starter Lambda triggered
3. Job record created in DynamoDB
4. Step Functions workflow started
5. Validator detects errors
6. Workflow transitions to error handling
7. Email sent with error details

---

## Notes

- All CSV files use pipe (`|`) as delimiter (Trading 212 format)
- Dates should be in format: `yyyy-MM-dd HH:mm:ss`
- Decimal values use dot (`.`) as separator
- Files should be UTF-8 encoded
- Maximum file size: 10 MB (configurable in Lambda)
