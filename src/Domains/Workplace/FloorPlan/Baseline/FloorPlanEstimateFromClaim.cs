namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Functions of floor plan estimate from claim.</summary>
public static class FloorPlanEstimateFromClaim
{
    public static Dictionary<string, double> SummarizeRegionalDeskClaimsForEstimate(List<RegionalDeskClaim> items)
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

    public static List<FloorPlanEstimate> DeriveFloorPlanEstimatesFromClaim(List<RegionalDeskClaim> items)
    {
        var outValue = new List<FloorPlanEstimate>();
        foreach (var item in items)
        {
            var derived = FloorPlanEstimate.NewFloorPlanEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitFloorPlanEstimate(FloorPlanEstimate item)
    {
        return LenientDeskProfilePolicy.ClassifyLenientDeskProfile(item.NetVolume());
    }
}
