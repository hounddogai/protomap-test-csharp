namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Functions of strict pulse survey variance from slot.</summary>
public static class StrictPulseSurveyVarianceFromSlot
{
    public static Dictionary<string, double> SummarizeManualActionPlanSlotsForVariance(List<ManualActionPlanSlot> items)
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

    public static List<StrictPulseSurveyVariance> DeriveStrictPulseSurveyVariancesFromSlot(List<ManualActionPlanSlot> items)
    {
        var outValue = new List<StrictPulseSurveyVariance>();
        foreach (var item in items)
        {
            var derived = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveStrictPulseSurveyVariance(StrictPulseSurveyVariance item)
    {
        return ActionPlanSchedulePolicy.ActionPlanScheduleWeight(9, true);
    }
}
