using System.Net;

namespace InvestTax.Lambda.EmailSender.Services;

/// <summary>
/// Static helper for generating email HTML and text templates
/// </summary>
public static class EmailTemplates
{
    /// <summary>
    /// Generate HTML body for success email
    /// </summary>
    public static string GenerateSuccessHtml(int year, string jobId, string reportContent)
    {
        var escapedReport = WebUtility.HtmlEncode(reportContent);
        
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; border-radius: 5px; }}
        .report {{ background-color: white; padding: 15px; border: 1px solid #ddd; border-radius: 3px; font-family: monospace; white-space: pre-wrap; font-size: 12px; overflow-x: auto; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>✓ Tax Calculation Complete</h1>
    </div>
    
    <div class=""content"">
        <h2>Polish Capital Gains Tax Report - {year}</h2>
        <p><strong>Job ID:</strong> {jobId}</p>
        <p><strong>Status:</strong> Successfully completed</p>
        
        <p>Your tax calculation has been completed successfully. Please review the detailed report below:</p>
        
        <div class=""report"">
{escapedReport}
        </div>
        
        <h3>Next Steps</h3>
        <ul>
            <li>Review the calculations carefully</li>
            <li>Consult with a tax professional before filing</li>
            <li>Use this report as reference for your PIT-38 form</li>
            <li>Keep this email for your records</li>
        </ul>
        
        <h3>Important Notes</h3>
        <ul>
            <li>This report uses FIFO (First In, First Out) cost basis methodology</li>
            <li>Exchange rates are sourced from the National Bank of Poland (NBP)</li>
            <li>Tax rate: 19% on capital gains (as per Polish tax law)</li>
        </ul>
    </div>
    
    <div class=""footer"">
        <p><strong>Disclaimer:</strong> This report is provided for informational purposes only and does not constitute tax advice.</p>
        <p>Please consult with a qualified tax professional to review your specific tax situation.</p>
        <p><em>InvestTax Calculator - Polish Capital Gains Tax Calculation Tool</em></p>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Generate plain text body for success email
    /// </summary>
    public static string GenerateSuccessText(int year, string jobId, string reportContent)
    {
        return $@"INVESTTAX CALCULATOR - TAX CALCULATION COMPLETE

Polish Capital Gains Tax Report - {year}
Job ID: {jobId}
Status: Successfully completed

Your tax calculation has been completed successfully. Please review the detailed report below:

{reportContent}

NEXT STEPS:
- Review the calculations carefully
- Consult with a tax professional before filing
- Use this report as reference for your PIT-38 form
- Keep this email for your records

IMPORTANT NOTES:
- This report uses FIFO (First In, First Out) cost basis methodology
- Exchange rates are sourced from the National Bank of Poland (NBP)
- Tax rate: 19% on capital gains (as per Polish tax law)

DISCLAIMER:
This report is provided for informational purposes only and does not constitute tax advice.
Please consult with a qualified tax professional to review your specific tax situation.

InvestTax Calculator - Polish Capital Gains Tax Calculation Tool
";
    }

    /// <summary>
    /// Generate HTML body for error email
    /// </summary>
    public static string GenerateErrorHtml(int year, string jobId, string errorStage, string errorMessage)
    {
        var escapedError = WebUtility.HtmlEncode(errorMessage);
        var escapedStage = WebUtility.HtmlEncode(errorStage);
        
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; border-radius: 5px; }}
        .error-box {{ background-color: #ffebee; padding: 15px; border-left: 4px solid #f44336; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>✗ Tax Calculation Failed</h1>
    </div>
    
    <div class=""content"">
        <h2>Processing Error - {year}</h2>
        <p><strong>Job ID:</strong> {jobId}</p>
        <p><strong>Status:</strong> Failed at {escapedStage} stage</p>
        
        <div class=""error-box"">
            <h3>Error Details</h3>
            <p>{escapedError}</p>
        </div>
        
        <h3>What to do next:</h3>
        <ul>
            <li>Check that your CSV file follows the required format</li>
            <li>Ensure all required columns are present and properly formatted</li>
            <li>Verify that dates are in the correct format (YYYY-MM-DD or MM/DD/YYYY)</li>
            <li>Confirm that currency codes are valid (USD, EUR, GBP, etc.)</li>
            <li>Make sure ISIN codes are exactly 12 characters</li>
        </ul>
        
        <h3>Common Issues:</h3>
        <ul>
            <li><strong>Validation errors:</strong> Check CSV headers and data types</li>
            <li><strong>Date errors:</strong> Ensure dates are within valid range and properly formatted</li>
            <li><strong>Currency errors:</strong> Verify currency codes are supported</li>
            <li><strong>FIFO errors:</strong> Cannot sell more shares than purchased</li>
        </ul>
        
        <p>If you continue to experience issues, please check your CSV file format and try again.</p>
    </div>
    
    <div class=""footer"">
        <p><em>InvestTax Calculator - Polish Capital Gains Tax Calculation Tool</em></p>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Generate plain text body for error email
    /// </summary>
    public static string GenerateErrorText(int year, string jobId, string errorStage, string errorMessage)
    {
        return $@"INVESTTAX CALCULATOR - PROCESSING FAILED

Polish Capital Gains Tax Report - {year}
Job ID: {jobId}
Status: Failed at {errorStage} stage

ERROR DETAILS:
{errorMessage}

WHAT TO DO NEXT:
- Check that your CSV file follows the required format
- Ensure all required columns are present and properly formatted
- Verify that dates are in the correct format (YYYY-MM-DD or MM/DD/YYYY)
- Confirm that currency codes are valid (USD, EUR, GBP, etc.)
- Make sure ISIN codes are exactly 12 characters

COMMON ISSUES:
- Validation errors: Check CSV headers and data types
- Date errors: Ensure dates are within valid range and properly formatted
- Currency errors: Verify currency codes are supported
- FIFO errors: Cannot sell more shares than purchased

If you continue to experience issues, please check your CSV file format and try again.

InvestTax Calculator - Polish Capital Gains Tax Calculation Tool
";
    }
}
