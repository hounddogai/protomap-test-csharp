namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive action plan mapping policy.</summary>
public static class AdaptiveActionPlanMappingPolicy
{
    public static List<string> MergeAdaptiveActionPlanMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double AdaptiveActionPlanMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adaptive action plan mapping score falls in.</summary>
    public static string ClassifyAdaptiveActionPlanMapping(double score)
    {
        if (score < 7.7)
        {
            return "low";
        }
        if (score < 40.6)
        {
            return "medium";
        }
        if (score < 64.6)
        {
            return "elevated";
        }
        if (score < 93.7)
        {
            return "high";
        }
        return "critical";
    }
}
