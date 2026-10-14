namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Functions of primary merit cycle rollup from preference.</summary>
public static class PrimaryMeritCycleRollupFromPreference
{
    public static Dictionary<string, double> SummarizeBenchmarkPreferencesForRollup(List<BenchmarkPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PrimaryMeritCycleRollup> DerivePrimaryMeritCycleRollupsFromPreference(List<BenchmarkPreference> items)
    {
        var outValue = new List<PrimaryMeritCycleRollup>();
        foreach (var item in items)
        {
            var derived = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPrimaryMeritCycleRollup(PrimaryMeritCycleRollup item)
    {
        return RollingBenchmarkProposalPolicy.NormalizeRollingBenchmarkProposalCode(item.Code);
    }
}
