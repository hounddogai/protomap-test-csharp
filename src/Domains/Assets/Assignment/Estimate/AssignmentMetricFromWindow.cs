namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Functions of assignment metric from window.</summary>
public static class AssignmentMetricFromWindow
{
    public static Dictionary<string, double> SummarizeSeasonalRefreshWindowsForMetric(List<SeasonalRefreshWindow> items)
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

    public static List<AssignmentMetric> DeriveAssignmentMetricsFromWindow(List<SeasonalRefreshWindow> items)
    {
        var outValue = new List<AssignmentMetric>();
        foreach (var item in items)
        {
            var derived = AssignmentMetric.NewAssignmentMetric(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAssignmentMetric(AssignmentMetric item)
    {
        return SeasonalRefreshWindowPolicy.SeasonalRefreshWindowBucket(item.WeightedTotal());
    }
}
