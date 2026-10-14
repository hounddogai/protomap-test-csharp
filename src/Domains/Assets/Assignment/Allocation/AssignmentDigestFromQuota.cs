namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Functions of assignment digest from quota.</summary>
public static class AssignmentDigestFromQuota
{
    public static Dictionary<string, double> SummarizeRefreshQuotasForDigest(List<RefreshQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<AssignmentDigest> DeriveAssignmentDigestsFromQuota(List<RefreshQuota> items)
    {
        var outValue = new List<AssignmentDigest>();
        foreach (var item in items)
        {
            var derived = AssignmentDigest.NewAssignmentDigest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAssignmentDigest(AssignmentDigest item)
    {
        return RefreshMetricPolicy.RefreshMetricBucket(item.NetValue());
    }
}
