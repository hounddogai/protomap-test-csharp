namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Functions of refresh schedule from policy.</summary>
public static class RefreshScheduleFromPolicy
{
    public static Dictionary<string, double> SummarizeFallbackRefreshPoliciesForSchedule(List<FallbackRefreshPolicy> items)
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

    public static List<RefreshSchedule> DeriveRefreshSchedulesFromPolicy(List<FallbackRefreshPolicy> items)
    {
        var outValue = new List<RefreshSchedule>();
        foreach (var item in items)
        {
            var derived = RefreshSchedule.NewRefreshSchedule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRefreshSchedule(RefreshSchedule item)
    {
        return WeightedRefreshScorePolicy.WeightedRefreshScoreBucket(item.EffectiveValue());
    }
}
