namespace Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Functions of strict succession plan tier policy.</summary>
public static class StrictSuccessionPlanTierPolicy
{
    public static int StrictSuccessionPlanTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a strict succession plan tier code in its canonical SSPT- form.</summary>
    public static string NormalizeStrictSuccessionPlanTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a strict succession plan tier score falls in.</summary>
    public static string ClassifyStrictSuccessionPlanTier(double score)
    {
        if (score < 1.6)
        {
            return "ok";
        }
        if (score < 42.2)
        {
            return "medium";
        }
        if (score < 49.1)
        {
            return "elevated";
        }
        if (score < 65.0)
        {
            return "minimal";
        }
        return "low";
    }
}
