namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Functions of headcount rule from plan.</summary>
public static class HeadcountRuleFromPlan
{
    public static Dictionary<string, double> SummarizeLegacyPositionPlansForRule(List<LegacyPositionPlan> items)
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

    public static List<HeadcountRule> DeriveHeadcountRulesFromPlan(List<LegacyPositionPlan> items)
    {
        var outValue = new List<HeadcountRule>();
        foreach (var item in items)
        {
            var derived = HeadcountRule.NewHeadcountRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignHeadcountRule(HeadcountRule item)
    {
        return PositionReviewPolicy.PositionReviewWeight(6, true);
    }
}
