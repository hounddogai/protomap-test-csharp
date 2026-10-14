namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Functions of laptop plan from batch.</summary>
public static class LaptopPlanFromBatch
{
    public static Dictionary<string, double> SummarizeRefreshBatchesForPlan(List<RefreshBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<LaptopPlan> DeriveLaptopPlansFromBatch(List<RefreshBatch> items)
    {
        var outValue = new List<LaptopPlan>();
        foreach (var item in items)
        {
            var derived = LaptopPlan.NewLaptopPlan(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeLaptopPlan(LaptopPlan item)
    {
        return RefreshBatchPolicy.NormalizeRefreshBatchCode(item.Code);
    }
}
