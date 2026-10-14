namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Functions of default assignment snapshot from audit.</summary>
public static class DefaultAssignmentSnapshotFromAudit
{
    public static Dictionary<string, double> SummarizeRefreshAuditsForSnapshot(List<RefreshAudit> items)
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

    public static List<DefaultAssignmentSnapshot> DeriveDefaultAssignmentSnapshotsFromAudit(List<RefreshAudit> items)
    {
        var outValue = new List<DefaultAssignmentSnapshot>();
        foreach (var item in items)
        {
            var derived = DefaultAssignmentSnapshot.NewDefaultAssignmentSnapshot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchDefaultAssignmentSnapshot(DefaultAssignmentSnapshot item)
    {
        return PendingRefreshMappingPolicy.ClassifyPendingRefreshMapping(item.EffectiveExposure());
    }
}
