namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>Functions of action plan mapping from adjustment.</summary>
public static class ActionPlanMappingFromAdjustment
{
    public static Dictionary<string, double> SummarizeGlobalPulseSurveyAdjustmentsForMapping(List<GlobalPulseSurveyAdjustment> items)
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

    public static List<ActionPlanMapping> DeriveActionPlanMappingsFromAdjustment(List<GlobalPulseSurveyAdjustment> items)
    {
        var outValue = new List<ActionPlanMapping>();
        foreach (var item in items)
        {
            var derived = ActionPlanMapping.NewActionPlanMapping(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceActionPlanMapping(ActionPlanMapping item)
    {
        return GlobalPulseSurveyAdjustmentPolicy.GlobalPulseSurveyAdjustmentBucket(item.AdjustedLoad());
    }
}
