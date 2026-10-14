namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Functions of capped merit cycle score from estimate.</summary>
public static class CappedMeritCycleScoreFromEstimate
{
    public static Dictionary<string, double> SummarizeRegionalBenchmarkEstimatesForScore(List<RegionalBenchmarkEstimate> items)
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

    public static List<CappedMeritCycleScore> DeriveCappedMeritCycleScoresFromEstimate(List<RegionalBenchmarkEstimate> items)
    {
        var outValue = new List<CappedMeritCycleScore>();
        foreach (var item in items)
        {
            var derived = CappedMeritCycleScore.NewCappedMeritCycleScore(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedMeritCycleScore(CappedMeritCycleScore item)
    {
        return RegionalBenchmarkEstimatePolicy.RegionalBenchmarkEstimateBucket(item.EffectiveVolume());
    }
}
