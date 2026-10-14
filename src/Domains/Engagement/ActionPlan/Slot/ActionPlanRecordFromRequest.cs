namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Functions of action plan record from request.</summary>
public static class ActionPlanRecordFromRequest
{
    public static Dictionary<string, double> SummarizeActionPlanRequestsForRecord(List<ActionPlanRequest> items)
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

    public static List<ActionPlanRecord> DeriveActionPlanRecordsFromRequest(List<ActionPlanRequest> items)
    {
        var outValue = new List<ActionPlanRecord>();
        foreach (var item in items)
        {
            var derived = ActionPlanRecord.NewActionPlanRecord(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateActionPlanRecord(ActionPlanRecord item)
    {
        return ActionPlanQuotaPolicy.NormalizeActionPlanQuotaCode(item.Code);
    }
}
