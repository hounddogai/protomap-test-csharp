namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Functions of scheduled headcount budget from mapping.</summary>
public static class ScheduledHeadcountBudgetFromMapping
{
    public static Dictionary<string, double> SummarizePrimaryScenarioMappingsForBudget(List<PrimaryScenarioMapping> items)
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

    public static List<ScheduledHeadcountBudget> DeriveScheduledHeadcountBudgetsFromMapping(List<PrimaryScenarioMapping> items)
    {
        var outValue = new List<ScheduledHeadcountBudget>();
        foreach (var item in items)
        {
            var derived = ScheduledHeadcountBudget.NewScheduledHeadcountBudget(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileScheduledHeadcountBudget(ScheduledHeadcountBudget item)
    {
        return SeasonalScenarioRequestPolicy.SeasonalScenarioRequestBucket(item.MeasuredValue());
    }
}
