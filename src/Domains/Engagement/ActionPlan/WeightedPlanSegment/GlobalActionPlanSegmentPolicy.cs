namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global action plan segment policy.</summary>
public static class GlobalActionPlanSegmentPolicy
{
    /// <summary>Names the band a global action plan segment score falls in.</summary>
    public static string ClassifyGlobalActionPlanSegment(double score)
    {
        if (score < 18.4)
        {
            return "medium";
        }
        if (score < 31.8)
        {
            return "elevated";
        }
        if (score < 92.0)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Puts a global action plan segment code in its canonical GAPS- form.</summary>
    public static string NormalizeGlobalActionPlanSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeGlobalActionPlanSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
