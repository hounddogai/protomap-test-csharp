namespace Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Functions of lenient refresh claim policy.</summary>
public static class LenientRefreshClaimPolicy
{
    /// <summary>Puts a lenient refresh claim code in its canonical LRC- form.</summary>
    public static string NormalizeLenientRefreshClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a lenient refresh claim score falls in.</summary>
    public static string ClassifyLenientRefreshClaim(double score)
    {
        if (score < 31.3)
        {
            return "medium";
        }
        if (score < 53.5)
        {
            return "ok";
        }
        if (score < 75.5)
        {
            return "low";
        }
        return "watch";
    }
}
