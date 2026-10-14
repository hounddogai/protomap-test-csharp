namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Functions of warranty quota from milestone.</summary>
public static class WarrantyQuotaFromMilestone
{
    public static Dictionary<string, double> SummarizeAdaptiveWarrantyMilestonesForQuota(List<AdaptiveWarrantyMilestone> items)
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

    public static List<WarrantyQuota> DeriveWarrantyQuotasFromMilestone(List<AdaptiveWarrantyMilestone> items)
    {
        var outValue = new List<WarrantyQuota>();
        foreach (var item in items)
        {
            var derived = WarrantyQuota.NewWarrantyQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchWarrantyQuota(WarrantyQuota item)
    {
        return WarrantyAllocationPolicy.WarrantyAllocationWeight(3, true);
    }
}
