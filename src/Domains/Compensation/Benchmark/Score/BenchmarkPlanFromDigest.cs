namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Functions of benchmark plan from digest.</summary>
public static class BenchmarkPlanFromDigest
{
    public static Dictionary<string, double> SummarizeLenientBonusPoolDigestsForPlan(List<LenientBonusPoolDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<BenchmarkPlan> DeriveBenchmarkPlansFromDigest(List<LenientBonusPoolDigest> items)
    {
        var outValue = new List<BenchmarkPlan>();
        foreach (var item in items)
        {
            var derived = BenchmarkPlan.NewBenchmarkPlan(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreBenchmarkPlan(BenchmarkPlan item)
    {
        return PrimaryBonusPoolTierPolicy.PrimaryBonusPoolTierWeight(5, true);
    }
}
