namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Orgdesign.CostModel.Metric;

/// <summary>Functions of span exception from metric.</summary>
public static class SpanExceptionFromMetric
{
    public static Dictionary<string, double> SummarizeCostModelMetricsForException(List<CostModelMetric> items)
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

    public static List<SpanException> DeriveSpanExceptionsFromMetric(List<CostModelMetric> items)
    {
        var outValue = new List<SpanException>();
        foreach (var item in items)
        {
            var derived = SpanException.NewSpanException(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveSpanException(SpanException item)
    {
        return CostModelMetricPolicy.CostModelMetricWeight(3, true);
    }
}
