namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Functions of desk snapshot from draft.</summary>
public static class DeskSnapshotFromDraft
{
    public static Dictionary<string, double> SummarizeCappedAccessBadgeDraftsForSnapshot(List<CappedAccessBadgeDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<DeskSnapshot> DeriveDeskSnapshotsFromDraft(List<CappedAccessBadgeDraft> items)
    {
        var outValue = new List<DeskSnapshot>();
        foreach (var item in items)
        {
            var derived = DeskSnapshot.NewDeskSnapshot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanDeskSnapshot(DeskSnapshot item)
    {
        return AccessBadgeAllocationPolicy.AccessBadgeAllocationBucket(item.NetVolume());
    }
}
