namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Functions of batched cost model batch from metric.</summary>
public static class BatchedCostModelBatchFromMetric
{
    public static Dictionary<string, double> SummarizeHeadcountMetricsForBatch(List<HeadcountMetric> items)
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

    public static List<BatchedCostModelBatch> DeriveBatchedCostModelBatchesFromMetric(List<HeadcountMetric> items)
    {
        var outValue = new List<BatchedCostModelBatch>();
        foreach (var item in items)
        {
            var derived = BatchedCostModelBatch.NewBatchedCostModelBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeBatchedCostModelBatch(BatchedCostModelBatch item)
    {
        return HeadcountMetricPolicy.HeadcountMetricBucket(item.AdjustedVolume());
    }
}
