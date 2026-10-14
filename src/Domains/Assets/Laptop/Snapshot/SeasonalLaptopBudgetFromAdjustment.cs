namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Functions of seasonal laptop budget from adjustment.</summary>
public static class SeasonalLaptopBudgetFromAdjustment
{
    public static Dictionary<string, double> SummarizePendingAssetReturnAdjustmentsForBudget(List<PendingAssetReturnAdjustment> items)
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

    public static List<SeasonalLaptopBudget> DeriveSeasonalLaptopBudgetsFromAdjustment(List<PendingAssetReturnAdjustment> items)
    {
        var outValue = new List<SeasonalLaptopBudget>();
        foreach (var item in items)
        {
            var derived = SeasonalLaptopBudget.NewSeasonalLaptopBudget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveSeasonalLaptopBudget(SeasonalLaptopBudget item)
    {
        return ManualAssetReturnBatchPolicy.ManualAssetReturnBatchWeight(5, true);
    }
}
