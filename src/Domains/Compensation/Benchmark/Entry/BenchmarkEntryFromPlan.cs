namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Functions of benchmark entry from plan.</summary>
public static class BenchmarkEntryFromPlan
{
    public static Dictionary<string, double> SummarizeMeritCyclePlansForEntry(List<MeritCyclePlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<BenchmarkEntry> DeriveBenchmarkEntriesFromPlan(List<MeritCyclePlan> items)
    {
        var outValue = new List<BenchmarkEntry>();
        foreach (var item in items)
        {
            var derived = BenchmarkEntry.NewBenchmarkEntry(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeBenchmarkEntry(BenchmarkEntry item)
    {
        return MeritCycleClaimPolicy.MeritCycleClaimWeight(9, true);
    }
}
