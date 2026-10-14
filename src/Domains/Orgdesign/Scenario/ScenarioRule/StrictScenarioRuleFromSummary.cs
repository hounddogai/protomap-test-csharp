namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Functions of strict scenario rule from summary.</summary>
public static class StrictScenarioRuleFromSummary
{
    public static Dictionary<string, double> SummarizeScenarioSummariesForRule(List<ScenarioSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<StrictScenarioRule> DeriveStrictScenarioRulesFromSummary(List<ScenarioSummary> items)
    {
        var outValue = new List<StrictScenarioRule>();
        foreach (var item in items)
        {
            var derived = StrictScenarioRule.NewStrictScenarioRule(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceStrictScenarioRule(StrictScenarioRule item)
    {
        return WeightedScenarioMappingPolicy.NormalizeWeightedScenarioMappingCode(item.Code);
    }
}
