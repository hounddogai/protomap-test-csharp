namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Functions of engagement snapshot from slot.</summary>
public static class EngagementSnapshotFromSlot
{
    public static Dictionary<string, double> SummarizeRateSlotsForSnapshot(List<RateSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<EngagementSnapshot> DeriveEngagementSnapshotsFromSlot(List<RateSlot> items)
    {
        var outValue = new List<EngagementSnapshot>();
        foreach (var item in items)
        {
            var derived = EngagementSnapshot.NewEngagementSnapshot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeEngagementSnapshot(EngagementSnapshot item)
    {
        return RateManifestPolicy.RateManifestBucket(item.EffectiveLoad());
    }
}
