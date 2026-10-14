namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Functions of lenient benchmark rollup from milestone.</summary>
public static class LenientBenchmarkRollupFromMilestone
{
    public static Dictionary<string, double> SummarizeStrictEquityGrantMilestonesForRollup(List<StrictEquityGrantMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<LenientBenchmarkRollup> DeriveLenientBenchmarkRollupsFromMilestone(List<StrictEquityGrantMilestone> items)
    {
        var outValue = new List<LenientBenchmarkRollup>();
        foreach (var item in items)
        {
            var derived = LenientBenchmarkRollup.NewLenientBenchmarkRollup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleLenientBenchmarkRollup(LenientBenchmarkRollup item)
    {
        return GlobalEquityGrantDraftPolicy.GlobalEquityGrantDraftWeight(9, true);
    }
}
