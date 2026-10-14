namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global booking tier policy.</summary>
public static class GlobalBookingTierPolicy
{
    public static int GlobalBookingTierSpan(int start, int end)
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
        return span / 3600;
    }

    public static List<string> MergeGlobalBookingTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a global booking tier score falls in.</summary>
    public static string ClassifyGlobalBookingTier(double score)
    {
        if (score < 22.8)
        {
            return "watch";
        }
        if (score < 64.9)
        {
            return "elevated";
        }
        if (score < 79.2)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Puts a global booking tier code in its canonical GBT- form.</summary>
    public static string NormalizeGlobalBookingTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
