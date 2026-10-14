namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal town hall metric policy.</summary>
public static class SeasonalTownHallMetricPolicy
{
    /// <summary>Names the band a seasonal town hall metric score falls in.</summary>
    public static string ClassifySeasonalTownHallMetric(double score)
    {
        if (score < 53.7)
        {
            return "watch";
        }
        if (score < 58.2)
        {
            return "low";
        }
        return "minimal";
    }

    public static List<string> MergeSeasonalTownHallMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
