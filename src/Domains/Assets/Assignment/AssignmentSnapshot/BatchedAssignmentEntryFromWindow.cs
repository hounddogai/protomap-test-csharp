namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Refresh.Window;

/// <summary>Functions of batched assignment entry from window.</summary>
public static class BatchedAssignmentEntryFromWindow
{
    public static Dictionary<string, double> SummarizeBatchedRefreshWindowsForEntry(List<BatchedRefreshWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<BatchedAssignmentEntry> DeriveBatchedAssignmentEntriesFromWindow(List<BatchedRefreshWindow> items)
    {
        var outValue = new List<BatchedAssignmentEntry>();
        foreach (var item in items)
        {
            var derived = BatchedAssignmentEntry.NewBatchedAssignmentEntry(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBatchedAssignmentEntry(BatchedAssignmentEntry item)
    {
        return BatchedRefreshWindowPolicy.BatchedRefreshWindowBucket(item.ProjectedVolume());
    }
}
