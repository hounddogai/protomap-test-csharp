namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Functions of headcount metric from metric.</summary>
public static class HeadcountMetricFromMetric
{
    public static Dictionary<string, double> SummarizeSpanMetricsForMetric(List<SpanMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<HeadcountMetric> DeriveHeadcountMetricsFromMetric(List<SpanMetric> items)
    {
        var outValue = new List<HeadcountMetric>();
        foreach (var item in items)
        {
            var derived = HeadcountMetric.NewHeadcountMetric(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveHeadcountMetric(HeadcountMetric item)
    {
        return LegacySpanAdjustmentPolicy.ClassifyLegacySpanAdjustment(item.ProjectedLoad());
    }
}
