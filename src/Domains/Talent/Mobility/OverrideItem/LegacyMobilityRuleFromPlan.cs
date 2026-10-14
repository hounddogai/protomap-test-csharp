namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Functions of legacy mobility rule from plan.</summary>
public static class LegacyMobilityRuleFromPlan
{
    public static Dictionary<string, double> SummarizeSkillProfilePlansForRule(List<SkillProfilePlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<LegacyMobilityRule> DeriveLegacyMobilityRulesFromPlan(List<SkillProfilePlan> items)
    {
        var outValue = new List<LegacyMobilityRule>();
        foreach (var item in items)
        {
            var derived = LegacyMobilityRule.NewLegacyMobilityRule(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLegacyMobilityRule(LegacyMobilityRule item)
    {
        return PendingSkillProfileTargetPolicy.ClassifyPendingSkillProfileTarget(item.WeightedValue());
    }
}
