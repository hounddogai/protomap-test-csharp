namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted merit cycle segment policy.</summary>
public static class WeightedMeritCycleSegmentPolicy
{
    /// <summary>Names the band a weighted merit cycle segment score falls in.</summary>
    public static string ClassifyWeightedMeritCycleSegment(double score)
    {
        if (score < 25.2)
        {
            return "low";
        }
        if (score < 70.7)
        {
            return "elevated";
        }
        if (score < 77.1)
        {
            return "medium";
        }
        return "minimal";
    }

    public static List<string> MergeWeightedMeritCycleSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a weighted merit cycle segment code in its canonical WMCS- form.</summary>
    public static string NormalizeWeightedMeritCycleSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WMCS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WMCS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int WeightedMeritCycleSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
