namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Functions of global mobility rule from budget.</summary>
public static class GlobalMobilityRuleFromBudget
{
    public static Dictionary<string, double> SummarizeSeasonalSuccessionPlanBudgetsForRule(List<SeasonalSuccessionPlanBudget> items)
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

    public static List<GlobalMobilityRule> DeriveGlobalMobilityRulesFromBudget(List<SeasonalSuccessionPlanBudget> items)
    {
        var outValue = new List<GlobalMobilityRule>();
        foreach (var item in items)
        {
            var derived = GlobalMobilityRule.NewGlobalMobilityRule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveGlobalMobilityRule(GlobalMobilityRule item)
    {
        return RollingSuccessionPlanLedgerPolicy.RollingSuccessionPlanLedgerWeight(5, true);
    }
}
