namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Functions of seasonal visitor checkpoint from snapshot.</summary>
public static class SeasonalVisitorCheckpointFromSnapshot
{
    public static Dictionary<string, double> SummarizeScheduledFloorPlanSnapshotsForCheckpoint(List<ScheduledFloorPlanSnapshot> items)
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

    public static List<SeasonalVisitorCheckpoint> DeriveSeasonalVisitorCheckpointsFromSnapshot(List<ScheduledFloorPlanSnapshot> items)
    {
        var outValue = new List<SeasonalVisitorCheckpoint>();
        foreach (var item in items)
        {
            var derived = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveSeasonalVisitorCheckpoint(SeasonalVisitorCheckpoint item)
    {
        return SeasonalFloorPlanBatchPolicy.SeasonalFloorPlanBatchWeight(5, true);
    }
}
