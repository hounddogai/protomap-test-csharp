namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Functions of scheduled floor plan snapshot from exception.</summary>
public static class ScheduledFloorPlanSnapshotFromException
{
    public static Dictionary<string, double> SummarizeDeskExceptionsForSnapshot(List<DeskException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<ScheduledFloorPlanSnapshot> DeriveScheduledFloorPlanSnapshotsFromException(List<DeskException> items)
    {
        var outValue = new List<ScheduledFloorPlanSnapshot>();
        foreach (var item in items)
        {
            var derived = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeScheduledFloorPlanSnapshot(ScheduledFloorPlanSnapshot item)
    {
        return CappedDeskAdjustmentPolicy.NormalizeCappedDeskAdjustmentCode(item.Code);
    }
}
