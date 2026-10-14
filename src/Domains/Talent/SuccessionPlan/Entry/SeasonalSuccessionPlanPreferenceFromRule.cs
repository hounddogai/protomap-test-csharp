namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Functions of seasonal succession plan preference from rule.</summary>
public static class SeasonalSuccessionPlanPreferenceFromRule
{
    public static Dictionary<string, double> SummarizeManualPoolRulesForPreference(List<ManualPoolRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<SeasonalSuccessionPlanPreference> DeriveSeasonalSuccessionPlanPreferencesFromRule(List<ManualPoolRule> items)
    {
        var outValue = new List<SeasonalSuccessionPlanPreference>();
        foreach (var item in items)
        {
            var derived = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveSeasonalSuccessionPlanPreference(SeasonalSuccessionPlanPreference item)
    {
        return RollingPoolEntryPolicy.NormalizeRollingPoolEntryCode(item.Code);
    }
}
