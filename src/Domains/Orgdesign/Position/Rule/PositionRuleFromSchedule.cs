namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Functions of position rule from schedule.</summary>
public static class PositionRuleFromSchedule
{
    public static Dictionary<string, double> SummarizeScenarioSchedulesForRule(List<ScenarioSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<PositionRule> DerivePositionRulesFromSchedule(List<ScenarioSchedule> items)
    {
        var outValue = new List<PositionRule>();
        foreach (var item in items)
        {
            var derived = PositionRule.NewPositionRule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPositionRule(PositionRule item)
    {
        return ScenarioSchedulePolicy.NormalizeScenarioScheduleCode(item.Code);
    }
}
