namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy career path entry policy.</summary>
public static class LegacyCareerPathEntryPolicy
{
    public static List<string> MergeLegacyCareerPathEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LegacyCareerPathEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string LegacyCareerPathEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a legacy career path entry code in its canonical LCPE- form.</summary>
    public static string NormalizeLegacyCareerPathEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LCPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LCPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
