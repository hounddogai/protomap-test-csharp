namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Functions of bonus pool score from milestone.</summary>
public static class BonusPoolScoreFromMilestone
{
    public static Dictionary<string, double> SummarizeAdaptiveBenchmarkMilestonesForScore(List<AdaptiveBenchmarkMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<BonusPoolScore> DeriveBonusPoolScoresFromMilestone(List<AdaptiveBenchmarkMilestone> items)
    {
        var outValue = new List<BonusPoolScore>();
        foreach (var item in items)
        {
            var derived = BonusPoolScore.NewBonusPoolScore(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBonusPoolScore(BonusPoolScore item)
    {
        return PrimaryBenchmarkClaimPolicy.PrimaryBenchmarkClaimBucket(item.WeightedValue());
    }
}
