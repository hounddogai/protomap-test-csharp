namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Functions of regional deduction metric from batch.</summary>
public static class RegionalDeductionMetricFromBatch
{
    public static Dictionary<string, double> SummarizeDeductionBatchesForMetric(List<DeductionBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<RegionalDeductionMetric> DeriveRegionalDeductionMetricsFromBatch(List<DeductionBatch> items)
    {
        var outValue = new List<RegionalDeductionMetric>();
        foreach (var item in items)
        {
            var derived = RegionalDeductionMetric.NewRegionalDeductionMetric(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRegionalDeductionMetric(RegionalDeductionMetric item)
    {
        return DeductionProfilePolicy.ClassifyDeductionProfile(item.ProjectedTotal());
    }
}
