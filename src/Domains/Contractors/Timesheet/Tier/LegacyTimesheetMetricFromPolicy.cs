namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Contractors.Rate.Notice;

/// <summary>Functions of legacy timesheet metric from policy.</summary>
public static class LegacyTimesheetMetricFromPolicy
{
    public static Dictionary<string, double> SummarizeRatePoliciesForMetric(List<RatePolicy> items)
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

    public static List<LegacyTimesheetMetric> DeriveLegacyTimesheetMetricsFromPolicy(List<RatePolicy> items)
    {
        var outValue = new List<LegacyTimesheetMetric>();
        foreach (var item in items)
        {
            var derived = LegacyTimesheetMetric.NewLegacyTimesheetMetric(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyLegacyTimesheetMetric(LegacyTimesheetMetric item)
    {
        return WeightedRateNoticePolicy.WeightedRateNoticeBucket(item.NetLoad());
    }
}
