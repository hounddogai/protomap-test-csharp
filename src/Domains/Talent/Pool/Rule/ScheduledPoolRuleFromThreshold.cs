namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Functions of scheduled pool rule from threshold.</summary>
public static class ScheduledPoolRuleFromThreshold
{
    public static Dictionary<string, double> SummarizeSkillProfileThresholdsForRule(List<SkillProfileThreshold> items)
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

    public static List<ScheduledPoolRule> DeriveScheduledPoolRulesFromThreshold(List<SkillProfileThreshold> items)
    {
        var outValue = new List<ScheduledPoolRule>();
        foreach (var item in items)
        {
            var derived = ScheduledPoolRule.NewScheduledPoolRule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreScheduledPoolRule(ScheduledPoolRule item)
    {
        return SkillProfileThresholdPolicy.SkillProfileThresholdWeight(9, true);
    }
}
