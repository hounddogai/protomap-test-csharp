namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Workplace.Desk.Rule;

/// <summary>Functions of strict access badge assignment from rule.</summary>
public static class StrictAccessBadgeAssignmentFromRule
{
    public static Dictionary<string, double> SummarizeDeskRulesForAssignment(List<DeskRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<StrictAccessBadgeAssignment> DeriveStrictAccessBadgeAssignmentsFromRule(List<DeskRule> items)
    {
        var outValue = new List<StrictAccessBadgeAssignment>();
        foreach (var item in items)
        {
            var derived = StrictAccessBadgeAssignment.NewStrictAccessBadgeAssignment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyStrictAccessBadgeAssignment(StrictAccessBadgeAssignment item)
    {
        return DeskRulePolicy.DeskRuleBucket(item.WeightedValue());
    }
}
