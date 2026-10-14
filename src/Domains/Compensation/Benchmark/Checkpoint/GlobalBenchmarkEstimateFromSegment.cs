namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Functions of global benchmark estimate from segment.</summary>
public static class GlobalBenchmarkEstimateFromSegment
{
    public static Dictionary<string, double> SummarizeEquityGrantSegmentsForEstimate(List<EquityGrantSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<GlobalBenchmarkEstimate> DeriveGlobalBenchmarkEstimatesFromSegment(List<EquityGrantSegment> items)
    {
        var outValue = new List<GlobalBenchmarkEstimate>();
        foreach (var item in items)
        {
            var derived = GlobalBenchmarkEstimate.NewGlobalBenchmarkEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveGlobalBenchmarkEstimate(GlobalBenchmarkEstimate item)
    {
        return EquityGrantSlotPolicy.EquityGrantSlotWeight(8, true);
    }
}
