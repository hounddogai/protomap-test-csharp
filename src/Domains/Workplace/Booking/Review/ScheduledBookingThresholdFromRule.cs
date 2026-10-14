namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Functions of scheduled booking threshold from rule.</summary>
public static class ScheduledBookingThresholdFromRule
{
    public static Dictionary<string, double> SummarizeLegacyFloorPlanRulesForThreshold(List<LegacyFloorPlanRule> items)
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

    public static List<ScheduledBookingThreshold> DeriveScheduledBookingThresholdsFromRule(List<LegacyFloorPlanRule> items)
    {
        var outValue = new List<ScheduledBookingThreshold>();
        foreach (var item in items)
        {
            var derived = ScheduledBookingThreshold.NewScheduledBookingThreshold(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignScheduledBookingThreshold(ScheduledBookingThreshold item)
    {
        return LegacyFloorPlanRulePolicy.LegacyFloorPlanRuleBucket(item.MeasuredValue());
    }
}
