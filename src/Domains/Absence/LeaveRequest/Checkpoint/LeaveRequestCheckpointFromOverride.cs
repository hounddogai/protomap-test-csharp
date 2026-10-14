namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Functions of leave request checkpoint from override.</summary>
public static class LeaveRequestCheckpointFromOverride
{
    public static Dictionary<string, double> SummarizeLeaveRequestOverridesForCheckpoint(List<LeaveRequestOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<LeaveRequestCheckpoint> DeriveLeaveRequestCheckpointsFromOverride(List<LeaveRequestOverride> items)
    {
        var outValue = new List<LeaveRequestCheckpoint>();
        foreach (var item in items)
        {
            var derived = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLeaveRequestCheckpoint(LeaveRequestCheckpoint item)
    {
        return LeaveRequestBaselinePolicy.ClassifyLeaveRequestBaseline(item.WeightedExposure());
    }
}
