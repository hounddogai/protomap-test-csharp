namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Functions of rolling pool exception from override.</summary>
public static class RollingPoolExceptionFromOverride
{
    public static Dictionary<string, double> SummarizeStrictMobilityOverridesForException(List<StrictMobilityOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<RollingPoolException> DeriveRollingPoolExceptionsFromOverride(List<StrictMobilityOverride> items)
    {
        var outValue = new List<RollingPoolException>();
        foreach (var item in items)
        {
            var derived = RollingPoolException.NewRollingPoolException(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRollingPoolException(RollingPoolException item)
    {
        return LegacyMobilityRulePolicy.ClassifyLegacyMobilityRule(item.AdjustedLoad());
    }
}
