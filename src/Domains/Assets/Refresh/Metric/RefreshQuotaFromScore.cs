namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Functions of refresh quota from score.</summary>
public static class RefreshQuotaFromScore
{
    public static Dictionary<string, double> SummarizeAssignmentScoresForQuota(List<AssignmentScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<RefreshQuota> DeriveRefreshQuotasFromScore(List<AssignmentScore> items)
    {
        var outValue = new List<RefreshQuota>();
        foreach (var item in items)
        {
            var derived = RefreshQuota.NewRefreshQuota(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileRefreshQuota(RefreshQuota item)
    {
        return AssignmentScorePolicy.AssignmentScoreBucket(item.EffectiveTotal());
    }
}
