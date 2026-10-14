namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Functions of scenario exception from budget.</summary>
public static class ScenarioExceptionFromBudget
{
    public static Dictionary<string, double> SummarizeRegionalHeadcountBudgetsForException(List<RegionalHeadcountBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<ScenarioException> DeriveScenarioExceptionsFromBudget(List<RegionalHeadcountBudget> items)
    {
        var outValue = new List<ScenarioException>();
        foreach (var item in items)
        {
            var derived = ScenarioException.NewScenarioException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateScenarioException(ScenarioException item)
    {
        return RegionalHeadcountBudgetPolicy.ClassifyRegionalHeadcountBudget(item.EffectiveExposure());
    }
}
