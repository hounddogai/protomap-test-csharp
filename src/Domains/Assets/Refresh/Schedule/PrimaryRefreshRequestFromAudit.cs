namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Assets.Assignment.Policy;

/// <summary>Functions of primary refresh request from audit.</summary>
public static class PrimaryRefreshRequestFromAudit
{
    public static Dictionary<string, double> SummarizeWeightedAssignmentAuditsForRequest(List<WeightedAssignmentAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<PrimaryRefreshRequest> DerivePrimaryRefreshRequestsFromAudit(List<WeightedAssignmentAudit> items)
    {
        var outValue = new List<PrimaryRefreshRequest>();
        foreach (var item in items)
        {
            var derived = PrimaryRefreshRequest.NewPrimaryRefreshRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePrimaryRefreshRequest(PrimaryRefreshRequest item)
    {
        return PendingAssignmentTierPolicy.PendingAssignmentTierBucket(item.AdjustedLoad());
    }
}
