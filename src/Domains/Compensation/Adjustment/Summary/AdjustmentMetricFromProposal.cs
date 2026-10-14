namespace Acme.Workforce.Compensation.Adjustment.Summary;

using Acme.Workforce.Compensation.Benchmark.Proposal;

/// <summary>Functions of adjustment metric from proposal.</summary>
public static class AdjustmentMetricFromProposal
{
    public static Dictionary<string, double> SummarizeLenientBenchmarkProposalsForMetric(List<LenientBenchmarkProposal> items)
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

    public static List<AdjustmentMetric> DeriveAdjustmentMetricsFromProposal(List<LenientBenchmarkProposal> items)
    {
        var outValue = new List<AdjustmentMetric>();
        foreach (var item in items)
        {
            var derived = AdjustmentMetric.NewAdjustmentMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAdjustmentMetric(AdjustmentMetric item)
    {
        return TieredBenchmarkSummaryPolicy.ClassifyTieredBenchmarkSummary(item.MeasuredValue());
    }
}
