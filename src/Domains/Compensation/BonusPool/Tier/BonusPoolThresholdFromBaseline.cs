namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Functions of bonus pool threshold from baseline.</summary>
public static class BonusPoolThresholdFromBaseline
{
    public static Dictionary<string, double> SummarizeCappedBenchmarkBaselinesForThreshold(List<CappedBenchmarkBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<BonusPoolThreshold> DeriveBonusPoolThresholdsFromBaseline(List<CappedBenchmarkBaseline> items)
    {
        var outValue = new List<BonusPoolThreshold>();
        foreach (var item in items)
        {
            var derived = BonusPoolThreshold.NewBonusPoolThreshold(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeBonusPoolThreshold(BonusPoolThreshold item)
    {
        return BenchmarkAdjustmentPolicy.BenchmarkAdjustmentWeight(7, true);
    }
}
