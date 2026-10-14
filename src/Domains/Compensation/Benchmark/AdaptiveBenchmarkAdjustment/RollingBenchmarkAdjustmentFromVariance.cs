namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Functions of rolling benchmark adjustment from variance.</summary>
public static class RollingBenchmarkAdjustmentFromVariance
{
    public static Dictionary<string, double> SummarizeStrictBonusPoolVariancesForAdjustment(List<StrictBonusPoolVariance> items)
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

    public static List<RollingBenchmarkAdjustment> DeriveRollingBenchmarkAdjustmentsFromVariance(List<StrictBonusPoolVariance> items)
    {
        var outValue = new List<RollingBenchmarkAdjustment>();
        foreach (var item in items)
        {
            var derived = RollingBenchmarkAdjustment.NewRollingBenchmarkAdjustment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankRollingBenchmarkAdjustment(RollingBenchmarkAdjustment item)
    {
        return ManualBonusPoolProposalPolicy.ManualBonusPoolProposalWeight(9, true);
    }
}
