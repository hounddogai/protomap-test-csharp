namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Engagement.ActionPlan.Snapshot;

/// <summary>Functions of global town hall checkpoint from snapshot.</summary>
public static class GlobalTownHallCheckpointFromSnapshot
{
    public static Dictionary<string, double> SummarizeActionPlanSnapshotsForCheckpoint(List<ActionPlanSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<GlobalTownHallCheckpoint> DeriveGlobalTownHallCheckpointsFromSnapshot(List<ActionPlanSnapshot> items)
    {
        var outValue = new List<GlobalTownHallCheckpoint>();
        foreach (var item in items)
        {
            var derived = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitGlobalTownHallCheckpoint(GlobalTownHallCheckpoint item)
    {
        return ActionPlanSnapshotPolicy.ActionPlanSnapshotBucket(item.ProjectedTotal());
    }
}
