namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Functions of floor plan request from threshold.</summary>
public static class FloorPlanRequestFromThreshold
{
    public static Dictionary<string, double> SummarizeRegionalAccessBadgeThresholdsForRequest(List<RegionalAccessBadgeThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<FloorPlanRequest> DeriveFloorPlanRequestsFromThreshold(List<RegionalAccessBadgeThreshold> items)
    {
        var outValue = new List<FloorPlanRequest>();
        foreach (var item in items)
        {
            var derived = FloorPlanRequest.NewFloorPlanRequest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceFloorPlanRequest(FloorPlanRequest item)
    {
        return WeightedAccessBadgeExceptionPolicy.NormalizeWeightedAccessBadgeExceptionCode(item.Code);
    }
}
