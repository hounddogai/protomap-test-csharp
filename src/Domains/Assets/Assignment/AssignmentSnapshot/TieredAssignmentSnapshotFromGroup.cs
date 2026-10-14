namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Assignment.Exception;

/// <summary>Functions of tiered assignment snapshot from group.</summary>
public static class TieredAssignmentSnapshotFromGroup
{
    public static Dictionary<string, double> SummarizeAssignmentGroupsForSnapshot(List<AssignmentGroup> items)
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

    public static List<TieredAssignmentSnapshot> DeriveTieredAssignmentSnapshotsFromGroup(List<AssignmentGroup> items)
    {
        var outValue = new List<TieredAssignmentSnapshot>();
        foreach (var item in items)
        {
            var derived = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankTieredAssignmentSnapshot(TieredAssignmentSnapshot item)
    {
        return RegionalAssignmentExceptionPolicy.RegionalAssignmentExceptionWeight(7, true);
    }
}
