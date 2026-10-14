namespace Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Functions of pending warranty quota policy.</summary>
public static class PendingWarrantyQuotaPolicy
{
    /// <summary>Names the band a pending warranty quota score falls in.</summary>
    public static string ClassifyPendingWarrantyQuota(double score)
    {
        if (score < 4.3)
        {
            return "medium";
        }
        if (score < 68.8)
        {
            return "critical";
        }
        if (score < 69.7)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Puts a pending warranty quota code in its canonical PWQ- form.</summary>
    public static string NormalizePendingWarrantyQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PWQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PWQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
