namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Functions of benchmark estimate from schedule.</summary>
public static class BenchmarkEstimateFromSchedule
{
    public static Dictionary<string, double> SummarizeBonusPoolSchedulesForEstimate(List<BonusPoolSchedule> items)
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

    public static List<BenchmarkEstimate> DeriveBenchmarkEstimatesFromSchedule(List<BonusPoolSchedule> items)
    {
        var outValue = new List<BenchmarkEstimate>();
        foreach (var item in items)
        {
            var derived = BenchmarkEstimate.NewBenchmarkEstimate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBenchmarkEstimate(BenchmarkEstimate item)
    {
        return BonusPoolGroupPolicy.NormalizeBonusPoolGroupCode(item.Code);
    }
}
