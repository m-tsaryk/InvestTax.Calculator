# User stories

## 1. Upload transaction file

- **ID**: GH-001
- **Description**: As an investor, I want to upload a pipe-delimited CSV so the system can calculate my PIT-38 taxes.
- **Acceptance criteria**:
  - System accepts files with the required header columns.
  - System rejects files with missing required columns and explains the issue.
  - System confirms successful file intake before processing.

## 2. Validate transaction data

- **ID**: GH-002
- **Description**: As an investor, I want validation errors that tell me which rows are invalid so I can fix my file.
- **Acceptance criteria**:
  - Validation errors identify row number and column name.
  - Errors include a clear description of the issue.
  - Processing stops if any required field is invalid.

## 3. Calculate PIT-38 totals

- **ID**: GH-003
- **Description**: As an investor, I want accurate PIT-38 totals so I can report my taxes correctly.
- **Acceptance criteria**:
  - Sells are matched to buys by ISIN using FIFO based on trade date.
  - All amounts are converted to PLN using NBP official exchange rates per trade date.
  - Transaction costs (commissions and fees) are included in cost basis and sale proceeds.
  - Results include total gains, total losses, and net taxable amount with proper rounding.

## 4. Verify NBP exchange rates

- **ID**: GH-004
- **Description**: As an investor, I want to know which NBP exchange rates were used so I can verify the calculation against official sources.
- **Acceptance criteria**:
  - Summary displays NBP exchange rate used for each transaction date.
  - System uses fallback to most recent published rate when NBP rate unavailable for specific date.
  - Summary indicates when fallback rate was used.

## 5. Receive summary by email

- **ID**: GH-005
- **Description**: As an investor, I want a readable email summary so I can understand and store the result.
- **Acceptance criteria**:
  - Email includes the final tax amount and the reporting year.
  - Email includes a plain-language explanation of how the result was computed.
  - Email includes all assumptions and disclaimer about informational purpose.
  - Email is sent to the address provided by the user.

## 6. Review detailed calculation breakdown

- **ID**: GH-006
- **Description**: As an investor, I want a detailed breakdown showing each matched buy-sell pair so I can audit the calculation and provide documentation if requested by tax authorities.
- **Acceptance criteria**:
  - Summary includes per-ISIN totals for gains and losses.
  - Summary shows each matched sell-to-buy pair with dates, quantities, prices, and costs.
  - Summary displays transaction costs included in each calculation.
  - Summary displays NBP exchange rates and converted PLN amounts.
  - Summary lists all key assumptions (FIFO, trade date usage, NBP rates).

## 7. Include transaction costs in calculations

- **ID**: GH-007
- **Description**: As an investor, I want commissions and fees included in my cost basis so my tax calculation is accurate per Polish tax rules.
- **Acceptance criteria**:
  - Buy transaction costs increase the cost basis.
  - Sell transaction costs reduce the sale proceeds.
  - Summary clearly shows costs included in each calculation.

## 8. Secure access to submitted files

- **ID**: GH-008
- **Description**: As a user, I want my uploaded data protected so only my report is generated and delivered to me.
- **Acceptance criteria**:
  - Upload and processing links are unique per user and expire.
  - Summary emails are sent only to the address provided by the user.
  - Stored files are encrypted at rest.

## 9. Receive failure details by email

- **ID**: GH-009
- **Description**: As an investor, I want an email explaining validation or calculation failures so I can fix errors quickly.
- **Acceptance criteria**:
  - Failure email identifies the error type and affected rows when applicable.
  - Failure email provides steps to fix the issue.
  - Failure email is sent to the address provided by the user.

## 10. Detect potential corporate actions

- **ID**: GH-010
- **Description**: As an investor, I want to be warned if the system detects unusual patterns that might indicate corporate actions so I can verify my data is correct.
- **Acceptance criteria**:
  - System warns if ISIN has drastically different share counts that might indicate a split.
  - System warns if same ISIN has transactions with very different per-share prices.
  - Warning is included in summary email with recommendation to verify transactions.
  - Calculation proceeds using provided data unless user corrects it.