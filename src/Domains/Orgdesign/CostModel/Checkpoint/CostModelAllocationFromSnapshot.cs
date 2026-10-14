namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Functions of cost model allocation from snapshot.</summary>
public static class CostModelAllocationFromSnapshot
{
    public static Dictionary<string, double> SummarizeCostModelSnapshotsForAllocation(List<CostModelSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<CostModelAllocation> DeriveCostModelAllocationsFromSnapshot(List<CostModelSnapshot> items)
    {
        var outValue = new List<CostModelAllocation>();
        foreach (var item in items)
        {
            var derived = CostModelAllocation.NewCostModelAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectCostModelAllocation(CostModelAllocation item)
    {
        return CostModelManifestPolicy.ClassifyCostModelManifest(item.MeasuredValue());
    }
}
