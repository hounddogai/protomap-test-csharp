namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Functions of weighted bonus pool quota from mapping.</summary>
public static class WeightedBonusPoolQuotaFromMapping
{
    public static Dictionary<string, double> SummarizeAdaptiveMeritCycleMappingsForQuota(List<AdaptiveMeritCycleMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<WeightedBonusPoolQuota> DeriveWeightedBonusPoolQuotasFromMapping(List<AdaptiveMeritCycleMapping> items)
    {
        var outValue = new List<WeightedBonusPoolQuota>();
        foreach (var item in items)
        {
            var derived = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveWeightedBonusPoolQuota(WeightedBonusPoolQuota item)
    {
        return AdaptiveMeritCycleMappingPolicy.AdaptiveMeritCycleMappingWeight(1, true);
    }
}
