namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Functions of rolling floor plan bundle from digest.</summary>
public static class RollingFloorPlanBundleFromDigest
{
    public static Dictionary<string, double> SummarizeDeskDigestsForBundle(List<DeskDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RollingFloorPlanBundle> DeriveRollingFloorPlanBundlesFromDigest(List<DeskDigest> items)
    {
        var outValue = new List<RollingFloorPlanBundle>();
        foreach (var item in items)
        {
            var derived = RollingFloorPlanBundle.NewRollingFloorPlanBundle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyRollingFloorPlanBundleExtra(RollingFloorPlanBundle item)
    {
        return DeskSummaryPolicy.DeskSummaryWeight(7, true);
    }
}
