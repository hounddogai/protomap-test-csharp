namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Functions of benchmark rollup from threshold.</summary>
public static class BenchmarkRollupFromThreshold
{
    public static Dictionary<string, double> SummarizeBatchedBonusPoolThresholdsForRollup(List<BatchedBonusPoolThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<BenchmarkRollup> DeriveBenchmarkRollupsFromThreshold(List<BatchedBonusPoolThreshold> items)
    {
        var outValue = new List<BenchmarkRollup>();
        foreach (var item in items)
        {
            var derived = BenchmarkRollup.NewBenchmarkRollup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankBenchmarkRollup(BenchmarkRollup item)
    {
        return BatchedBonusPoolThresholdPolicy.ClassifyBatchedBonusPoolThreshold(item.EffectiveLoad());
    }
}
