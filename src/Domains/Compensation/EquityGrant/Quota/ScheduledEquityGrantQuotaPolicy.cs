namespace Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>Functions of scheduled equity grant quota policy.</summary>
public static class ScheduledEquityGrantQuotaPolicy
{
    /// <summary>Names the band a scheduled equity grant quota score falls in.</summary>
    public static string ClassifyScheduledEquityGrantQuota(double score)
    {
        if (score < 58.0)
        {
            return "ok";
        }
        if (score < 92.8)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Puts a scheduled equity grant quota code in its canonical SEGQ- form.</summary>
    public static string NormalizeScheduledEquityGrantQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SEGQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SEGQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
