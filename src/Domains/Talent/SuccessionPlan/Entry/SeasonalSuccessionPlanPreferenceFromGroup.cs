namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Functions of seasonal succession plan preference from group.</summary>
public static class SeasonalSuccessionPlanPreferenceFromGroup
{
    public static Dictionary<string, double> SummarizeRegionalSkillProfileGroupsForPreference(List<RegionalSkillProfileGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<SeasonalSuccessionPlanPreference> DeriveSeasonalSuccessionPlanPreferencesFromGroup(List<RegionalSkillProfileGroup> items)
    {
        var outValue = new List<SeasonalSuccessionPlanPreference>();
        foreach (var item in items)
        {
            var derived = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceSeasonalSuccessionPlanPreference(SeasonalSuccessionPlanPreference item)
    {
        return SkillProfileBudgetPolicy.SkillProfileBudgetWeight(8, true);
    }
}
