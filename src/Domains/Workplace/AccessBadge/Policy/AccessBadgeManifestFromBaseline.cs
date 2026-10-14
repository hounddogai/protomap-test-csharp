namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Functions of access badge manifest from baseline.</summary>
public static class AccessBadgeManifestFromBaseline
{
    public static Dictionary<string, double> SummarizeRegionalVisitorBaselinesForManifest(List<RegionalVisitorBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AccessBadgeManifest> DeriveAccessBadgeManifestsFromBaseline(List<RegionalVisitorBaseline> items)
    {
        var outValue = new List<AccessBadgeManifest>();
        foreach (var item in items)
        {
            var derived = AccessBadgeManifest.NewAccessBadgeManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAccessBadgeManifest(AccessBadgeManifest item)
    {
        return AdaptiveVisitorCyclePolicy.AdaptiveVisitorCycleBucket(item.MeasuredValue());
    }
}
