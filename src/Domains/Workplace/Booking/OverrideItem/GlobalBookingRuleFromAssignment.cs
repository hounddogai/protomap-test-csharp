namespace Acme.Workforce.Workplace.Booking.OverrideItem;

using Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>Functions of global booking rule from assignment.</summary>
public static class GlobalBookingRuleFromAssignment
{
    public static Dictionary<string, double> SummarizeManualFloorPlanAssignmentsForRule(List<ManualFloorPlanAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<GlobalBookingRule> DeriveGlobalBookingRulesFromAssignment(List<ManualFloorPlanAssignment> items)
    {
        var outValue = new List<GlobalBookingRule>();
        foreach (var item in items)
        {
            var derived = GlobalBookingRule.NewGlobalBookingRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeGlobalBookingRule(GlobalBookingRule item)
    {
        return FloorPlanReviewPolicy.FloorPlanReviewWeight(2, true);
    }
}
