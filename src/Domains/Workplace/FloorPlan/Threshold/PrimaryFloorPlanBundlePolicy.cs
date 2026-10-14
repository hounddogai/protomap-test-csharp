namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary floor plan bundle policy.</summary>
public static class PrimaryFloorPlanBundlePolicy
{
    public static List<string> MergePrimaryFloorPlanBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a primary floor plan bundle score falls in.</summary>
    public static string ClassifyPrimaryFloorPlanBundle(double score)
    {
        if (score < 3.7)
        {
            return "minimal";
        }
        if (score < 18.8)
        {
            return "elevated";
        }
        if (score < 26.1)
        {
            return "critical";
        }
        return "ok";
    }
}
