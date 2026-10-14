namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Workplace.Visitor.Target;

/// <summary>Functions of floor plan manifest from target.</summary>
public static class FloorPlanManifestFromTarget
{
    public static Dictionary<string, double> SummarizeVisitorTargetsForManifest(List<VisitorTarget> items)
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

    public static List<FloorPlanManifest> DeriveFloorPlanManifestsFromTarget(List<VisitorTarget> items)
    {
        var outValue = new List<FloorPlanManifest>();
        foreach (var item in items)
        {
            var derived = FloorPlanManifest.NewFloorPlanManifest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateFloorPlanManifest(FloorPlanManifest item)
    {
        return VisitorTargetPolicy.VisitorTargetBucket(item.EffectiveLoad());
    }
}
