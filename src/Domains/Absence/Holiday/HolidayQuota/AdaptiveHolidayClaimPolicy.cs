namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive holiday claim policy.</summary>
public static class AdaptiveHolidayClaimPolicy
{
    /// <summary>Names the band a adaptive holiday claim score falls in.</summary>
    public static string ClassifyAdaptiveHolidayClaim(double score)
    {
        if (score < 42.7)
        {
            return "minimal";
        }
        if (score < 74.3)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Puts a adaptive holiday claim code in its canonical AHC- form.</summary>
    public static string NormalizeAdaptiveHolidayClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AHC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AHC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAdaptiveHolidayClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int AdaptiveHolidayClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
