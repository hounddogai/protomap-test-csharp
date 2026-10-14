namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Functions of weighted scenario group from schedule.</summary>
public static class WeightedScenarioGroupFromSchedule
{
    public static Dictionary<string, double> SummarizeHeadcountSchedulesForGroup(List<HeadcountSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<WeightedScenarioGroup> DeriveWeightedScenarioGroupsFromSchedule(List<HeadcountSchedule> items)
    {
        var outValue = new List<WeightedScenarioGroup>();
        foreach (var item in items)
        {
            var derived = WeightedScenarioGroup.NewWeightedScenarioGroup(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeWeightedScenarioGroup(WeightedScenarioGroup item)
    {
        return HeadcountTierPolicy.HeadcountTierWeight(4, true);
    }
}
