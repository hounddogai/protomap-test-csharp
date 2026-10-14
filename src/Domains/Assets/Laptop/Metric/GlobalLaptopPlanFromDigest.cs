namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Functions of global laptop plan from digest.</summary>
public static class GlobalLaptopPlanFromDigest
{
    public static Dictionary<string, double> SummarizeSeasonalWarrantyDigestsForPlan(List<SeasonalWarrantyDigest> items)
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

    public static List<GlobalLaptopPlan> DeriveGlobalLaptopPlansFromDigest(List<SeasonalWarrantyDigest> items)
    {
        var outValue = new List<GlobalLaptopPlan>();
        foreach (var item in items)
        {
            var derived = GlobalLaptopPlan.NewGlobalLaptopPlan(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectGlobalLaptopPlan(GlobalLaptopPlan item)
    {
        return PrimaryWarrantyRollupPolicy.PrimaryWarrantyRollupWeight(3, true);
    }
}
