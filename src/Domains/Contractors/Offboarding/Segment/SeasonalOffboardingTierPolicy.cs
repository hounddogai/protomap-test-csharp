namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal offboarding tier policy.</summary>
public static class SeasonalOffboardingTierPolicy
{
    public static double SeasonalOffboardingTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal offboarding tier score falls in.</summary>
    public static string ClassifySeasonalOffboardingTier(double score)
    {
        if (score < 8.6)
        {
            return "high";
        }
        if (score < 15.6)
        {
            return "ok";
        }
        if (score < 18.1)
        {
            return "elevated";
        }
        if (score < 90.0)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Puts a seasonal offboarding tier code in its canonical SOT- form.</summary>
    public static string NormalizeSeasonalOffboardingTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SeasonalOffboardingTierSpan(int start, int end)
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
        return span / 60;
    }
}
