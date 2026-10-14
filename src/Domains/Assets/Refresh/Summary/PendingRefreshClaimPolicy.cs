namespace Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Functions of pending refresh claim policy.</summary>
public static class PendingRefreshClaimPolicy
{
    /// <summary>Puts a pending refresh claim code in its canonical PRC- form.</summary>
    public static string NormalizePendingRefreshClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a pending refresh claim score falls in.</summary>
    public static string ClassifyPendingRefreshClaim(double score)
    {
        if (score < 4.8)
        {
            return "high";
        }
        if (score < 22.3)
        {
            return "critical";
        }
        if (score < 29.4)
        {
            return "low";
        }
        if (score < 41.9)
        {
            return "watch";
        }
        return "ok";
    }
}
