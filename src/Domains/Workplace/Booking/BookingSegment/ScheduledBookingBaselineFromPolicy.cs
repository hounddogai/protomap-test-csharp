namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Functions of scheduled booking baseline from policy.</summary>
public static class ScheduledBookingBaselineFromPolicy
{
    public static Dictionary<string, double> SummarizeVisitorPoliciesForBaseline(List<VisitorPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<ScheduledBookingBaseline> DeriveScheduledBookingBaselinesFromPolicy(List<VisitorPolicy> items)
    {
        var outValue = new List<ScheduledBookingBaseline>();
        foreach (var item in items)
        {
            var derived = ScheduledBookingBaseline.NewScheduledBookingBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignScheduledBookingBaseline(ScheduledBookingBaseline item)
    {
        return VisitorAgreementPolicy.ClassifyVisitorAgreement(item.ProjectedTotal());
    }
}
