namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Functions of regional refresh request from quota.</summary>
public static class RegionalRefreshRequestFromQuota
{
    public static Dictionary<string, double> SummarizeAssignmentQuotasForRequest(List<AssignmentQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<RegionalRefreshRequest> DeriveRegionalRefreshRequestsFromQuota(List<AssignmentQuota> items)
    {
        var outValue = new List<RegionalRefreshRequest>();
        foreach (var item in items)
        {
            var derived = RegionalRefreshRequest.NewRegionalRefreshRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRegionalRefreshRequest(RegionalRefreshRequest item)
    {
        return ScheduledAssignmentClaimPolicy.ScheduledAssignmentClaimBucket(item.AdjustedLoad());
    }
}
