namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Functions of manual balance metric from rollup.</summary>
public static class ManualBalanceMetricFromRollup
{
    public static Dictionary<string, double> SummarizeScheduledLeaveRequestRollupsForMetric(List<ScheduledLeaveRequestRollup> items)
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

    public static List<ManualBalanceMetric> DeriveManualBalanceMetricsFromRollup(List<ScheduledLeaveRequestRollup> items)
    {
        var outValue = new List<ManualBalanceMetric>();
        foreach (var item in items)
        {
            var derived = ManualBalanceMetric.NewManualBalanceMetric(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileManualBalanceMetric(ManualBalanceMetric item)
    {
        return LeaveRequestClaimPolicy.LeaveRequestClaimBucket(item.NetVolume());
    }
}
