namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Functions of global pool cycle from preference.</summary>
public static class GlobalPoolCycleFromPreference
{
    public static Dictionary<string, double> SummarizePoolPreferencesForCycle(List<PoolPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<GlobalPoolCycle> DeriveGlobalPoolCyclesFromPreference(List<PoolPreference> items)
    {
        var outValue = new List<GlobalPoolCycle>();
        foreach (var item in items)
        {
            var derived = GlobalPoolCycle.NewGlobalPoolCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveGlobalPoolCycle(GlobalPoolCycle item)
    {
        return PoolPreferencePolicy.PoolPreferenceWeight(6, true);
    }
}
