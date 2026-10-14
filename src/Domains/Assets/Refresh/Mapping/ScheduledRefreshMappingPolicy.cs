namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled refresh mapping policy.</summary>
public static class ScheduledRefreshMappingPolicy
{
    /// <summary>Names the band a scheduled refresh mapping score falls in.</summary>
    public static string ClassifyScheduledRefreshMapping(double score)
    {
        if (score < 50.2)
        {
            return "low";
        }
        if (score < 88.4)
        {
            return "critical";
        }
        if (score < 96.2)
        {
            return "high";
        }
        return "watch";
    }

    public static List<string> MergeScheduledRefreshMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
