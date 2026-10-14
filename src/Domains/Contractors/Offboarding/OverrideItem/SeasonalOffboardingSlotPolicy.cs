namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal offboarding slot policy.</summary>
public static class SeasonalOffboardingSlotPolicy
{
    /// <summary>Puts a seasonal offboarding slot code in its canonical SOS- form.</summary>
    public static string NormalizeSeasonalOffboardingSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SeasonalOffboardingSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a seasonal offboarding slot score falls in.</summary>
    public static string ClassifySeasonalOffboardingSlot(double score)
    {
        if (score < 17.9)
        {
            return "medium";
        }
        if (score < 58.3)
        {
            return "elevated";
        }
        if (score < 81.9)
        {
            return "low";
        }
        if (score < 91.4)
        {
            return "critical";
        }
        return "watch";
    }

    public static double SeasonalOffboardingSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
