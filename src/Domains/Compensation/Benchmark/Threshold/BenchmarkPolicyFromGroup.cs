namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Compensation.BonusPool.Group;

/// <summary>Functions of benchmark policy from group.</summary>
public static class BenchmarkPolicyFromGroup
{
    public static Dictionary<string, double> SummarizeStrictBonusPoolGroupsForPolicy(List<StrictBonusPoolGroup> items)
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

    public static List<BenchmarkPolicy> DeriveBenchmarkPoliciesFromGroup(List<StrictBonusPoolGroup> items)
    {
        var outValue = new List<BenchmarkPolicy>();
        foreach (var item in items)
        {
            var derived = BenchmarkPolicy.NewBenchmarkPolicy(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchBenchmarkPolicy(BenchmarkPolicy item)
    {
        return AdaptiveBonusPoolCheckpointPolicy.ClassifyAdaptiveBonusPoolCheckpoint(item.EffectiveVolume());
    }
}
