namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy visitor milestone policy.</summary>
public static class LegacyVisitorMilestonePolicy
{
    public static int LegacyVisitorMilestoneSpan(int start, int end)
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

    public static List<string> MergeLegacyVisitorMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy visitor milestone score falls in.</summary>
    public static string ClassifyLegacyVisitorMilestone(double score)
    {
        if (score < 8.0)
        {
            return "ok";
        }
        if (score < 25.4)
        {
            return "minimal";
        }
        if (score < 85.2)
        {
            return "watch";
        }
        if (score < 97.6)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a legacy visitor milestone code in its canonical LVM- form.</summary>
    public static string NormalizeLegacyVisitorMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LVM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LVM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
