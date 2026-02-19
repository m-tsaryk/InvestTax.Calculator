namespace InvestTax.Core.Enums;

/// <summary>
/// Represents the processing status of a tax calculation job
/// </summary>
public enum JobStatus
{
    /// <summary>Job created, awaiting validation</summary>
    Created,
    
    /// <summary>CSV validation in progress</summary>
    Validating,
    
    /// <summary>Data normalization in progress</summary>
    Normalizing,
    
    /// <summary>Fetching NBP exchange rates</summary>
    FetchingRates,
    
    /// <summary>Tax calculation in progress</summary>
    Calculating,
    
    /// <summary>Generating report</summary>
    GeneratingReport,
    
    /// <summary>Sending email</summary>
    SendingEmail,
    
    /// <summary>Job completed successfully</summary>
    Completed,
    
    /// <summary>Job failed with error</summary>
    Failed
}
