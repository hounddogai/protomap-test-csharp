namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Functions of action plan rollup from milestone.</summary>
public static class ActionPlanRollupFromMilestone
{
    public static Dictionary<string, double> SummarizeManualActionPlanMilestonesForRollup(List<ManualActionPlanMilestone> items)
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

    public static List<ActionPlanRollup> DeriveActionPlanRollupsFromMilestone(List<ManualActionPlanMilestone> items)
    {
        var outValue = new List<ActionPlanRollup>();
        foreach (var item in items)
        {
            var derived = ActionPlanRollup.NewActionPlanRollup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyActionPlanRollupExtra(ActionPlanRollup item)
    {
        return ActionPlanAuditPolicy.NormalizeActionPlanAuditCode(item.Code);
    }
}
