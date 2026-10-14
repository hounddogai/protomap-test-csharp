namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal floor plan record policy.</summary>
public static class SeasonalFloorPlanRecordPolicy
{
    public static List<string> MergeSeasonalFloorPlanRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int SeasonalFloorPlanRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a seasonal floor plan record score falls in.</summary>
    public static string ClassifySeasonalFloorPlanRecord(double score)
    {
        if (score < 55.1)
        {
            return "elevated";
        }
        if (score < 75.6)
        {
            return "watch";
        }
        if (score < 84.5)
        {
            return "minimal";
        }
        return "medium";
    }
}
