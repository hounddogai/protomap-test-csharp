namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Functions of batched adjustment tier from estimate.</summary>
public static class BatchedAdjustmentTierFromEstimate
{
    public static Dictionary<string, double> SummarizeRegionalBenchmarkEstimatesForTier(List<RegionalBenchmarkEstimate> items)
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

    public static List<BatchedAdjustmentTier> DeriveBatchedAdjustmentTiersFromEstimate(List<RegionalBenchmarkEstimate> items)
    {
        var outValue = new List<BatchedAdjustmentTier>();
        foreach (var item in items)
        {
            var derived = BatchedAdjustmentTier.NewBatchedAdjustmentTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyBatchedAdjustmentTier(BatchedAdjustmentTier item)
    {
        return ManualBenchmarkMetricPolicy.ClassifyManualBenchmarkMetric(item.NetLoad());
    }
}
