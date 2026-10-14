namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Functions of strict laptop tier from adjustment.</summary>
public static class StrictLaptopTierFromAdjustment
{
    public static Dictionary<string, double> SummarizeRefreshAdjustmentsForTier(List<RefreshAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<StrictLaptopTier> DeriveStrictLaptopTiersFromAdjustment(List<RefreshAdjustment> items)
    {
        var outValue = new List<StrictLaptopTier>();
        foreach (var item in items)
        {
            var derived = StrictLaptopTier.NewStrictLaptopTier(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeStrictLaptopTier(StrictLaptopTier item)
    {
        return RefreshPlanPolicy.ClassifyRefreshPlan(item.AdjustedExposure());
    }
}
