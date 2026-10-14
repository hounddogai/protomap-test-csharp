namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>Functions of capped benchmark baseline from bundle.</summary>
public static class CappedBenchmarkBaselineFromBundle
{
    public static Dictionary<string, double> SummarizeAdjustmentBundlesForBaseline(List<AdjustmentBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<CappedBenchmarkBaseline> DeriveCappedBenchmarkBaselinesFromBundle(List<AdjustmentBundle> items)
    {
        var outValue = new List<CappedBenchmarkBaseline>();
        foreach (var item in items)
        {
            var derived = CappedBenchmarkBaseline.NewCappedBenchmarkBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedBenchmarkBaseline(CappedBenchmarkBaseline item)
    {
        return AdjustmentAuditPolicy.ClassifyAdjustmentAudit(item.AdjustedLoad());
    }
}
