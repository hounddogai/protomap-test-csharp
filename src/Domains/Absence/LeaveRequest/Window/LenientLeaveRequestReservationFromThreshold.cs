namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Functions of lenient leave request reservation from threshold.</summary>
public static class LenientLeaveRequestReservationFromThreshold
{
    public static Dictionary<string, double> SummarizeAdaptiveReturnPlanThresholdsForReservation(List<AdaptiveReturnPlanThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<LenientLeaveRequestReservation> DeriveLenientLeaveRequestReservationsFromThreshold(List<AdaptiveReturnPlanThreshold> items)
    {
        var outValue = new List<LenientLeaveRequestReservation>();
        foreach (var item in items)
        {
            var derived = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanLenientLeaveRequestReservation(LenientLeaveRequestReservation item)
    {
        return ManualReturnPlanMetricPolicy.ManualReturnPlanMetricBucket(item.NetLoad());
    }
}
