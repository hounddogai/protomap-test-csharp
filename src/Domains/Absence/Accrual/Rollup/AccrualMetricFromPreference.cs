namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Functions of accrual metric from preference.</summary>
public static class AccrualMetricFromPreference
{
    public static Dictionary<string, double> SummarizeRollingLeaveRequestPreferencesForMetric(List<RollingLeaveRequestPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<AccrualMetric> DeriveAccrualMetricsFromPreference(List<RollingLeaveRequestPreference> items)
    {
        var outValue = new List<AccrualMetric>();
        foreach (var item in items)
        {
            var derived = AccrualMetric.NewAccrualMetric(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeAccrualMetric(AccrualMetric item)
    {
        return LeaveRequestBundlePolicy.LeaveRequestBundleWeight(4, true);
    }
}
