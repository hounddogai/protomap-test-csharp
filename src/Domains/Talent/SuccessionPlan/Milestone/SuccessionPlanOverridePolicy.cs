namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan override policy.</summary>
public static class SuccessionPlanOverridePolicy
{
    /// <summary>Puts a succession plan override code in its canonical SPO- form.</summary>
    public static string NormalizeSuccessionPlanOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SuccessionPlanOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a succession plan override score falls in.</summary>
    public static string ClassifySuccessionPlanOverride(double score)
    {
        if (score < 31.8)
        {
            return "ok";
        }
        if (score < 46.0)
        {
            return "elevated";
        }
        return "medium";
    }
}
