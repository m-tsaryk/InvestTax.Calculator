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
  - Sells are matched to buys by ISIN using FIFO.
  - All amounts are converted to PLN using public FX rates per transaction date.
  - Results include total gains, total losses, and net taxable amount.

## 4. Receive summary by email

- **ID**: GH-004
- **Description**: As an investor, I want a readable email summary so I can understand and store the result.
- **Acceptance criteria**:
  - Email includes the final tax amount and the reporting year.
  - Email includes a plain-language explanation of how the result was computed.
  - Email is sent to the address provided by the user.

## 5. Review calculation breakdown

- **ID**: GH-005
- **Description**: As an investor, I want a breakdown by ISIN so I can audit the calculation.
- **Acceptance criteria**:
  - Summary includes per-ISIN totals for gains and losses.
  - Summary includes totals for each matched sell-to-buy set.
  - Summary lists key assumptions (FIFO, FX rate usage).

## 6. Secure access to submitted files

- **ID**: GH-006
- **Description**: As a user, I want my uploaded data protected so only my report is generated and delivered to me.
- **Acceptance criteria**:
  - Upload and processing links are unique per user and expire.
  - Summary emails are sent only to the address provided by the user.
  - Stored files are encrypted at rest.

## 7. Receive failure details by email

- **ID**: GH-007
- **Description**: As an investor, I want an email explaining validation or calculation failures so I can fix errors quickly.
- **Acceptance criteria**:
  - Failure email identifies the error type and affected rows when applicable.
  - Failure email provides steps to fix the issue.
  - Failure email is sent to the address provided by the user.