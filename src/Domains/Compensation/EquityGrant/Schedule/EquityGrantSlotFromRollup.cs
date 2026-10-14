namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

/// <summary>Functions of equity grant slot from rollup.</summary>
public static class EquityGrantSlotFromRollup
{
    public static Dictionary<string, double> SummarizeBenchmarkRollupsForSlot(List<BenchmarkRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<EquityGrantSlot> DeriveEquityGrantSlotsFromRollup(List<BenchmarkRollup> items)
    {
        var outValue = new List<EquityGrantSlot>();
        foreach (var item in items)
        {
            var derived = EquityGrantSlot.NewEquityGrantSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchEquityGrantSlot(EquityGrantSlot item)
    {
        return GlobalBenchmarkProfilePolicy.GlobalBenchmarkProfileWeight(4, true);
    }
}
