namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Functions of weighted timesheet metric from digest.</summary>
public static class WeightedTimesheetMetricFromDigest
{
    public static Dictionary<string, double> SummarizeDefaultEngagementDigestsForMetric(List<DefaultEngagementDigest> items)
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

    public static List<WeightedTimesheetMetric> DeriveWeightedTimesheetMetricsFromDigest(List<DefaultEngagementDigest> items)
    {
        var outValue = new List<WeightedTimesheetMetric>();
        foreach (var item in items)
        {
            var derived = WeightedTimesheetMetric.NewWeightedTimesheetMetric(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeWeightedTimesheetMetric(WeightedTimesheetMetric item)
    {
        return RollingEngagementForecastPolicy.RollingEngagementForecastBucket(item.EffectiveExposure());
    }
}
