namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Functions of default bonus pool cycle from proposal.</summary>
public static class DefaultBonusPoolCycleFromProposal
{
    public static Dictionary<string, double> SummarizeBenchmarkProposalsForCycle(List<BenchmarkProposal> items)
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

    public static List<DefaultBonusPoolCycle> DeriveDefaultBonusPoolCyclesFromProposal(List<BenchmarkProposal> items)
    {
        var outValue = new List<DefaultBonusPoolCycle>();
        foreach (var item in items)
        {
            var derived = DefaultBonusPoolCycle.NewDefaultBonusPoolCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateDefaultBonusPoolCycle(DefaultBonusPoolCycle item)
    {
        return BenchmarkForecastPolicy.BenchmarkForecastWeight(2, true);
    }
}
