namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Functions of fallback booking target from policy.</summary>
public static class FallbackBookingTargetFromPolicy
{
    public static Dictionary<string, double> SummarizeFloorPlanPoliciesForTarget(List<FloorPlanPolicy> items)
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

    public static List<FallbackBookingTarget> DeriveFallbackBookingTargetsFromPolicy(List<FloorPlanPolicy> items)
    {
        var outValue = new List<FallbackBookingTarget>();
        foreach (var item in items)
        {
            var derived = FallbackBookingTarget.NewFallbackBookingTarget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyFallbackBookingTargetExtra(FallbackBookingTarget item)
    {
        return FloorPlanPolicyPolicy.FloorPlanPolicyBucket(item.AdjustedExposure());
    }
}
