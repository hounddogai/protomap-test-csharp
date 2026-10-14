namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Functions of offboarding rule from override.</summary>
public static class OffboardingRuleFromOverride
{
    public static Dictionary<string, double> SummarizeRegionalEngagementOverridesForRule(List<RegionalEngagementOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<OffboardingRule> DeriveOffboardingRulesFromOverride(List<RegionalEngagementOverride> items)
    {
        var outValue = new List<OffboardingRule>();
        foreach (var item in items)
        {
            var derived = OffboardingRule.NewOffboardingRule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignOffboardingRule(OffboardingRule item)
    {
        return EngagementTargetPolicy.EngagementTargetWeight(9, true);
    }
}
