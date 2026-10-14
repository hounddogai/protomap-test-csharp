namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Functions of visitor rule from plan.</summary>
public static class VisitorRuleFromPlan
{
    public static Dictionary<string, double> SummarizeLenientBookingPlansForRule(List<LenientBookingPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<VisitorRule> DeriveVisitorRulesFromPlan(List<LenientBookingPlan> items)
    {
        var outValue = new List<VisitorRule>();
        foreach (var item in items)
        {
            var derived = VisitorRule.NewVisitorRule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateVisitorRule(VisitorRule item)
    {
        return ManualBookingAuditPolicy.ManualBookingAuditWeight(6, true);
    }
}
