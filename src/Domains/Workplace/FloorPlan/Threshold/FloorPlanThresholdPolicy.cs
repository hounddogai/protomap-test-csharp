namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of floor plan threshold policy.</summary>
public static class FloorPlanThresholdPolicy
{
    public static int FloorPlanThresholdSpan(int start, int end)
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
        return span / 3600;
    }

    public static List<string> MergeFloorPlanThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a floor plan threshold score falls in.</summary>
    public static string ClassifyFloorPlanThreshold(double score)
    {
        if (score < 58.4)
        {
            return "low";
        }
        if (score < 63.7)
        {
            return "medium";
        }
        if (score < 81.1)
        {
            return "ok";
        }
        if (score < 86.3)
        {
            return "watch";
        }
        return "elevated";
    }
}
