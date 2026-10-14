namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal floor plan variance policy.</summary>
public static class SeasonalFloorPlanVariancePolicy
{
    /// <summary>Names the band a seasonal floor plan variance score falls in.</summary>
    public static string ClassifySeasonalFloorPlanVariance(double score)
    {
        if (score < 29.4)
        {
            return "high";
        }
        if (score < 92.2)
        {
            return "critical";
        }
        return "medium";
    }

    public static double SeasonalFloorPlanVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeSeasonalFloorPlanVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
