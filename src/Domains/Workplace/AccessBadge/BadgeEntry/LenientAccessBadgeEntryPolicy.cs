namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient access badge entry policy.</summary>
public static class LenientAccessBadgeEntryPolicy
{
    /// <summary>Puts a lenient access badge entry code in its canonical LABE- form.</summary>
    public static string NormalizeLenientAccessBadgeEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LABE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LABE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientAccessBadgeEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a lenient access badge entry score falls in.</summary>
    public static string ClassifyLenientAccessBadgeEntry(double score)
    {
        if (score < 22.1)
        {
            return "high";
        }
        if (score < 25.0)
        {
            return "elevated";
        }
        if (score < 88.8)
        {
            return "minimal";
        }
        return "low";
    }

    public static double LenientAccessBadgeEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
