namespace InvestTax.Lambda.NBPClient.Models;

/// <summary>
/// Result of the rate fetching process
/// </summary>
public class RateFetchResult
{
    /// <summary>S3 key where rate map JSON is stored</summary>
    public string RateMapKey { get; set; } = string.Empty;
    
    /// <summary>Number of unique rates fetched</summary>
    public int TotalRatesFetched { get; set; }
    
    /// <summary>Currencies for which rates were fetched</summary>
    public List<string> Currencies { get; set; } = new();
}
