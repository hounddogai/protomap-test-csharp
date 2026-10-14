namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Compensation.Benchmark.Proposal;

/// <summary>Functions of pending bonus pool allocation from summary.</summary>
public static class PendingBonusPoolAllocationFromSummary
{
    public static Dictionary<string, double> SummarizeTieredBenchmarkSummariesForAllocation(List<TieredBenchmarkSummary> items)
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

    public static List<PendingBonusPoolAllocation> DerivePendingBonusPoolAllocationsFromSummary(List<TieredBenchmarkSummary> items)
    {
        var outValue = new List<PendingBonusPoolAllocation>();
        foreach (var item in items)
        {
            var derived = PendingBonusPoolAllocation.NewPendingBonusPoolAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPendingBonusPoolAllocation(PendingBonusPoolAllocation item)
    {
        return TieredBenchmarkSummaryPolicy.ClassifyTieredBenchmarkSummary(item.NetLoad());
    }
}
