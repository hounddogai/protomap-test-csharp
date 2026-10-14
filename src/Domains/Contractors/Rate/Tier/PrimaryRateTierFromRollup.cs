namespace Acme.Workforce.Contractors.Rate.Tier;

using Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>Functions of primary rate tier from rollup.</summary>
public static class PrimaryRateTierFromRollup
{
    public static Dictionary<string, double> SummarizeEngagementRollupsForTier(List<EngagementRollup> items)
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

    public static List<PrimaryRateTier> DerivePrimaryRateTiersFromRollup(List<EngagementRollup> items)
    {
        var outValue = new List<PrimaryRateTier>();
        foreach (var item in items)
        {
            var derived = PrimaryRateTier.NewPrimaryRateTier(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimatePrimaryRateTier(PrimaryRateTier item)
    {
        return EngagementRollupPolicy.EngagementRollupWeight(3, true);
    }
}
