namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped action plan request policy.</summary>
public static class CappedActionPlanRequestPolicy
{
    /// <summary>Puts a capped action plan request code in its canonical CAPR- form.</summary>
    public static string NormalizeCappedActionPlanRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CAPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CAPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a capped action plan request score falls in.</summary>
    public static string ClassifyCappedActionPlanRequest(double score)
    {
        if (score < 3.3)
        {
            return "medium";
        }
        if (score < 98.5)
        {
            return "high";
        }
        return "low";
    }

    public static double CappedActionPlanRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
