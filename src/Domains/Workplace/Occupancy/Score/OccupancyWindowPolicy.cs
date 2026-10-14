namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of occupancy window policy.</summary>
public static class OccupancyWindowPolicy
{
    /// <summary>Names the band a occupancy window score falls in.</summary>
    public static string ClassifyOccupancyWindow(double score)
    {
        if (score < 61.1)
        {
            return "critical";
        }
        if (score < 69.1)
        {
            return "high";
        }
        if (score < 81.0)
        {
            return "low";
        }
        return "minimal";
    }

    public static List<string> MergeOccupancyWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string OccupancyWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
