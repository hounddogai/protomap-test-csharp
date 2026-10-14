namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Functions of tiered pool exception from rule.</summary>
public static class TieredPoolExceptionFromRule
{
    public static Dictionary<string, double> SummarizeGlobalMobilityRulesForException(List<GlobalMobilityRule> items)
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

    public static List<TieredPoolException> DeriveTieredPoolExceptionsFromRule(List<GlobalMobilityRule> items)
    {
        var outValue = new List<TieredPoolException>();
        foreach (var item in items)
        {
            var derived = TieredPoolException.NewTieredPoolException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeTieredPoolException(TieredPoolException item)
    {
        return RollingMobilityAllocationPolicy.RollingMobilityAllocationWeight(6, true);
    }
}
