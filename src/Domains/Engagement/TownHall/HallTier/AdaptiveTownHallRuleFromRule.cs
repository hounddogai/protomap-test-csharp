namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallRule;

/// <summary>Functions of adaptive town hall rule from rule.</summary>
public static class AdaptiveTownHallRuleFromRule
{
    public static Dictionary<string, double> SummarizeBatchedTownHallRulesForRule(List<BatchedTownHallRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<AdaptiveTownHallRule> DeriveAdaptiveTownHallRulesFromRule(List<BatchedTownHallRule> items)
    {
        var outValue = new List<AdaptiveTownHallRule>();
        foreach (var item in items)
        {
            var derived = AdaptiveTownHallRule.NewAdaptiveTownHallRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileAdaptiveTownHallRule(AdaptiveTownHallRule item)
    {
        return TownHallDraftPolicy.TownHallDraftWeight(1, true);
    }
}
