namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Functions of fallback access badge override from manifest.</summary>
public static class FallbackAccessBadgeOverrideFromManifest
{
    public static Dictionary<string, double> SummarizeOccupancyManifestsForOverride(List<OccupancyManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<FallbackAccessBadgeOverride> DeriveFallbackAccessBadgeOverridesFromManifest(List<OccupancyManifest> items)
    {
        var outValue = new List<FallbackAccessBadgeOverride>();
        foreach (var item in items)
        {
            var derived = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeFallbackAccessBadgeOverride(FallbackAccessBadgeOverride item)
    {
        return OccupancyManifestPolicy.NormalizeOccupancyManifestCode(item.Code);
    }
}
