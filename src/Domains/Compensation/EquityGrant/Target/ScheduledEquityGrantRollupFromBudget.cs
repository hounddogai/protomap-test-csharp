namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Functions of scheduled equity grant rollup from budget.</summary>
public static class ScheduledEquityGrantRollupFromBudget
{
    public static Dictionary<string, double> SummarizeBenchmarkBudgetsForRollup(List<BenchmarkBudget> items)
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

    public static List<ScheduledEquityGrantRollup> DeriveScheduledEquityGrantRollupsFromBudget(List<BenchmarkBudget> items)
    {
        var outValue = new List<ScheduledEquityGrantRollup>();
        foreach (var item in items)
        {
            var derived = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignScheduledEquityGrantRollup(ScheduledEquityGrantRollup item)
    {
        return RollingBenchmarkProfilePolicy.RollingBenchmarkProfileWeight(4, true);
    }
}
