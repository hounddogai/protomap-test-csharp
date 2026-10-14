namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Functions of tiered return plan threshold policy.</summary>
public static class TieredReturnPlanThresholdPolicy
{
    /// <summary>Names the band a tiered return plan threshold score falls in.</summary>
    public static string ClassifyTieredReturnPlanThreshold(double score)
    {
        if (score < 14.7)
        {
            return "medium";
        }
        if (score < 47.6)
        {
            return "high";
        }
        if (score < 98.1)
        {
            return "ok";
        }
        return "minimal";
    }

    public static int TieredReturnPlanThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a tiered return plan threshold code in its canonical TRPT- form.</summary>
    public static string NormalizeTieredReturnPlanThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TRPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TRPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
