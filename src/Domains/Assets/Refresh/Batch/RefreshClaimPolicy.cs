namespace Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Functions of refresh claim policy.</summary>
public static class RefreshClaimPolicy
{
    /// <summary>Names the band a refresh claim score falls in.</summary>
    public static string ClassifyRefreshClaim(double score)
    {
        if (score < 26.8)
        {
            return "critical";
        }
        if (score < 31.9)
        {
            return "minimal";
        }
        if (score < 58.1)
        {
            return "ok";
        }
        if (score < 97.3)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Puts a refresh claim code in its canonical RC- form.</summary>
    public static string NormalizeRefreshClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
