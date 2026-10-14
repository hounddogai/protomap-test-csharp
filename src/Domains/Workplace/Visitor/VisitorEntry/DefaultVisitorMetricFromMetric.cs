namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Functions of default visitor metric from metric.</summary>
public static class DefaultVisitorMetricFromMetric
{
    public static Dictionary<string, double> SummarizeTieredDeskMetricsForMetric(List<TieredDeskMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<DefaultVisitorMetric> DeriveDefaultVisitorMetricsFromMetric(List<TieredDeskMetric> items)
    {
        var outValue = new List<DefaultVisitorMetric>();
        foreach (var item in items)
        {
            var derived = DefaultVisitorMetric.NewDefaultVisitorMetric(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateDefaultVisitorMetric(DefaultVisitorMetric item)
    {
        return PendingDeskAllocationPolicy.ClassifyPendingDeskAllocation(item.ProjectedExposure());
    }
}
