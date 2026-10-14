namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Functions of merit cycle snapshot from record.</summary>
public static class MeritCycleSnapshotFromRecord
{
    public static Dictionary<string, double> SummarizeRollingMeritCycleRecordsForSnapshot(List<RollingMeritCycleRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<MeritCycleSnapshot> DeriveMeritCycleSnapshotsFromRecord(List<RollingMeritCycleRecord> items)
    {
        var outValue = new List<MeritCycleSnapshot>();
        foreach (var item in items)
        {
            var derived = MeritCycleSnapshot.NewMeritCycleSnapshot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleMeritCycleSnapshot(MeritCycleSnapshot item)
    {
        return MeritCycleBatchPolicy.ClassifyMeritCycleBatch(item.ProjectedValue());
    }
}
