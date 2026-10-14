namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Assets.Refresh.Window;

/// <summary>Functions of warranty checkpoint from cycle.</summary>
public static class WarrantyCheckpointFromCycle
{
    public static Dictionary<string, double> SummarizeRegionalRefreshCyclesForCheckpoint(List<RegionalRefreshCycle> items)
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

    public static List<WarrantyCheckpoint> DeriveWarrantyCheckpointsFromCycle(List<RegionalRefreshCycle> items)
    {
        var outValue = new List<WarrantyCheckpoint>();
        foreach (var item in items)
        {
            var derived = WarrantyCheckpoint.NewWarrantyCheckpoint(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileWarrantyCheckpoint(WarrantyCheckpoint item)
    {
        return BatchedRefreshWindowPolicy.BatchedRefreshWindowBucket(item.NetLoad());
    }
}
