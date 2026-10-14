namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Functions of pulse survey budget from milestone.</summary>
public static class PulseSurveyBudgetFromMilestone
{
    public static Dictionary<string, double> SummarizeManualActionPlanMilestonesForBudget(List<ManualActionPlanMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PulseSurveyBudget> DerivePulseSurveyBudgetsFromMilestone(List<ManualActionPlanMilestone> items)
    {
        var outValue = new List<PulseSurveyBudget>();
        foreach (var item in items)
        {
            var derived = PulseSurveyBudget.NewPulseSurveyBudget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePulseSurveyBudget(PulseSurveyBudget item)
    {
        return ManualActionPlanMilestonePolicy.NormalizeManualActionPlanMilestoneCode(item.Code);
    }
}
