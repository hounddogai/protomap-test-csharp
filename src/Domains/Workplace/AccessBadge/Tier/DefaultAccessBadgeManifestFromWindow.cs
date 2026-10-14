namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Functions of default access badge manifest from window.</summary>
public static class DefaultAccessBadgeManifestFromWindow
{
    public static Dictionary<string, double> SummarizeOccupancyWindowsForManifest(List<OccupancyWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<DefaultAccessBadgeManifest> DeriveDefaultAccessBadgeManifestsFromWindow(List<OccupancyWindow> items)
    {
        var outValue = new List<DefaultAccessBadgeManifest>();
        foreach (var item in items)
        {
            var derived = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeDefaultAccessBadgeManifest(DefaultAccessBadgeManifest item)
    {
        return OccupancyWindowPolicy.OccupancyWindowBucket(item.WeightedValue());
    }
}
