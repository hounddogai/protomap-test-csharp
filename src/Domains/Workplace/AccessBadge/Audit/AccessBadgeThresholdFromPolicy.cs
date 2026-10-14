namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Functions of access badge threshold from policy.</summary>
public static class AccessBadgeThresholdFromPolicy
{
    public static Dictionary<string, double> SummarizeOccupancyPoliciesForThreshold(List<OccupancyPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<AccessBadgeThreshold> DeriveAccessBadgeThresholdsFromPolicy(List<OccupancyPolicy> items)
    {
        var outValue = new List<AccessBadgeThreshold>();
        foreach (var item in items)
        {
            var derived = AccessBadgeThreshold.NewAccessBadgeThreshold(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAccessBadgeThreshold(AccessBadgeThreshold item)
    {
        return OccupancyPreferencePolicy.NormalizeOccupancyPreferenceCode(item.Code);
    }
}
