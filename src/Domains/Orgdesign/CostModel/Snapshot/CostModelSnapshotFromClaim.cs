namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Functions of cost model snapshot from claim.</summary>
public static class CostModelSnapshotFromClaim
{
    public static Dictionary<string, double> SummarizeDefaultSpanClaimsForSnapshot(List<DefaultSpanClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<CostModelSnapshot> DeriveCostModelSnapshotsFromClaim(List<DefaultSpanClaim> items)
    {
        var outValue = new List<CostModelSnapshot>();
        foreach (var item in items)
        {
            var derived = CostModelSnapshot.NewCostModelSnapshot(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileCostModelSnapshot(CostModelSnapshot item)
    {
        return SpanSnapshotPolicy.ClassifySpanSnapshot(item.ProjectedValue());
    }
}
