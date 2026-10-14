namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Functions of refresh checkpoint from estimate.</summary>
public static class RefreshCheckpointFromEstimate
{
    public static Dictionary<string, double> SummarizeScheduledAssignmentEstimatesForCheckpoint(List<ScheduledAssignmentEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<RefreshCheckpoint> DeriveRefreshCheckpointsFromEstimate(List<ScheduledAssignmentEstimate> items)
    {
        var outValue = new List<RefreshCheckpoint>();
        foreach (var item in items)
        {
            var derived = RefreshCheckpoint.NewRefreshCheckpoint(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitRefreshCheckpoint(RefreshCheckpoint item)
    {
        return AssignmentManifestPolicy.AssignmentManifestBucket(item.MeasuredValue());
    }
}
