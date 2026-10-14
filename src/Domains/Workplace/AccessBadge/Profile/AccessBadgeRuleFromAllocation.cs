namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Functions of access badge rule from allocation.</summary>
public static class AccessBadgeRuleFromAllocation
{
    public static Dictionary<string, double> SummarizeDeskAllocationsForRule(List<DeskAllocation> items)
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

    public static List<AccessBadgeRule> DeriveAccessBadgeRulesFromAllocation(List<DeskAllocation> items)
    {
        var outValue = new List<AccessBadgeRule>();
        foreach (var item in items)
        {
            var derived = AccessBadgeRule.NewAccessBadgeRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileAccessBadgeRule(AccessBadgeRule item)
    {
        return DeskPolicyPolicy.DeskPolicyBucket(item.MeasuredValue());
    }
}
