namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Functions of batched garnishment metric from cycle.</summary>
public static class BatchedGarnishmentMetricFromCycle
{
    public static Dictionary<string, double> SummarizeGarnishmentCyclesForMetric(List<GarnishmentCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<BatchedGarnishmentMetric> DeriveBatchedGarnishmentMetricsFromCycle(List<GarnishmentCycle> items)
    {
        var outValue = new List<BatchedGarnishmentMetric>();
        foreach (var item in items)
        {
            var derived = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateBatchedGarnishmentMetric(BatchedGarnishmentMetric item)
    {
        return GarnishmentMetricPolicy.GarnishmentMetricWeight(7, true);
    }
}
