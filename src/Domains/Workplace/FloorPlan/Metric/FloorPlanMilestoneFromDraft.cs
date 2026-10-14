namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Functions of floor plan milestone from draft.</summary>
public static class FloorPlanMilestoneFromDraft
{
    public static Dictionary<string, double> SummarizeSeasonalAccessBadgeDraftsForMilestone(List<SeasonalAccessBadgeDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<FloorPlanMilestone> DeriveFloorPlanMilestonesFromDraft(List<SeasonalAccessBadgeDraft> items)
    {
        var outValue = new List<FloorPlanMilestone>();
        foreach (var item in items)
        {
            var derived = FloorPlanMilestone.NewFloorPlanMilestone(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveFloorPlanMilestone(FloorPlanMilestone item)
    {
        return WeightedAccessBadgeLedgerPolicy.ClassifyWeightedAccessBadgeLedger(item.WeightedExposure());
    }
}
