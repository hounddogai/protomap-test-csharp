namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Functions of floor plan claim from profile.</summary>
public static class FloorPlanClaimFromProfile
{
    public static Dictionary<string, double> SummarizePendingVisitorProfilesForClaim(List<PendingVisitorProfile> items)
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

    public static List<FloorPlanClaim> DeriveFloorPlanClaimsFromProfile(List<PendingVisitorProfile> items)
    {
        var outValue = new List<FloorPlanClaim>();
        foreach (var item in items)
        {
            var derived = FloorPlanClaim.NewFloorPlanClaim(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitFloorPlanClaim(FloorPlanClaim item)
    {
        return PendingVisitorProfilePolicy.PendingVisitorProfileBucket(item.ProjectedValue());
    }
}
