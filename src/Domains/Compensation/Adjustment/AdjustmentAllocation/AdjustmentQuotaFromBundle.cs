namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Functions of adjustment quota from bundle.</summary>
public static class AdjustmentQuotaFromBundle
{
    public static Dictionary<string, double> SummarizeMeritCycleBundlesForQuota(List<MeritCycleBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<AdjustmentQuota> DeriveAdjustmentQuotasFromBundle(List<MeritCycleBundle> items)
    {
        var outValue = new List<AdjustmentQuota>();
        foreach (var item in items)
        {
            var derived = AdjustmentQuota.NewAdjustmentQuota(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAdjustmentQuota(AdjustmentQuota item)
    {
        return MeritCycleBundlePolicy.NormalizeMeritCycleBundleCode(item.Code);
    }
}
