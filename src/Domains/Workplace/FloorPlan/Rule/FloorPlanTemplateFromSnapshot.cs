namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Functions of floor plan template from snapshot.</summary>
public static class FloorPlanTemplateFromSnapshot
{
    public static Dictionary<string, double> SummarizeFloorPlanSnapshotsForTemplate(List<FloorPlanSnapshot> items)
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

    public static List<FloorPlanTemplate> DeriveFloorPlanTemplatesFromSnapshot(List<FloorPlanSnapshot> items)
    {
        var outValue = new List<FloorPlanTemplate>();
        foreach (var item in items)
        {
            var derived = FloorPlanTemplate.NewFloorPlanTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreFloorPlanTemplate(FloorPlanTemplate item)
    {
        return AdaptiveFloorPlanDraftPolicy.ClassifyAdaptiveFloorPlanDraft(item.AdjustedVolume());
    }
}
