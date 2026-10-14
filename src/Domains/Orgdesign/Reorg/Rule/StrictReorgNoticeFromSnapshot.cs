namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Functions of strict reorg notice from snapshot.</summary>
public static class StrictReorgNoticeFromSnapshot
{
    public static Dictionary<string, double> SummarizeCostModelSnapshotsForNotice(List<CostModelSnapshot> items)
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

    public static List<StrictReorgNotice> DeriveStrictReorgNoticesFromSnapshot(List<CostModelSnapshot> items)
    {
        var outValue = new List<StrictReorgNotice>();
        foreach (var item in items)
        {
            var derived = StrictReorgNotice.NewStrictReorgNotice(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveStrictReorgNotice(StrictReorgNotice item)
    {
        return CostModelSnapshotPolicy.NormalizeCostModelSnapshotCode(item.Code);
    }
}
