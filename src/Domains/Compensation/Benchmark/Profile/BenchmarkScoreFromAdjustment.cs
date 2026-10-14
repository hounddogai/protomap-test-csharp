namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Functions of benchmark score from adjustment.</summary>
public static class BenchmarkScoreFromAdjustment
{
    public static Dictionary<string, double> SummarizeBenchmarkAdjustmentsForScore(List<BenchmarkAdjustment> items)
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

    public static List<BenchmarkScore> DeriveBenchmarkScoresFromAdjustment(List<BenchmarkAdjustment> items)
    {
        var outValue = new List<BenchmarkScore>();
        foreach (var item in items)
        {
            var derived = BenchmarkScore.NewBenchmarkScore(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreBenchmarkScore(BenchmarkScore item)
    {
        return WeightedBenchmarkMappingPolicy.WeightedBenchmarkMappingBucket(item.MeasuredValue());
    }
}
