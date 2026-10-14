namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Functions of capped occupancy policy from snapshot.</summary>
public static class CappedOccupancyPolicyFromSnapshot
{
    public static Dictionary<string, double> SummarizeScheduledFloorPlanSnapshotsForPolicy(List<ScheduledFloorPlanSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<CappedOccupancyPolicy> DeriveCappedOccupancyPoliciesFromSnapshot(List<ScheduledFloorPlanSnapshot> items)
    {
        var outValue = new List<CappedOccupancyPolicy>();
        foreach (var item in items)
        {
            var derived = CappedOccupancyPolicy.NewCappedOccupancyPolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeCappedOccupancyPolicy(CappedOccupancyPolicy item)
    {
        return BatchedFloorPlanSnapshotPolicy.NormalizeBatchedFloorPlanSnapshotCode(item.Code);
    }
}
