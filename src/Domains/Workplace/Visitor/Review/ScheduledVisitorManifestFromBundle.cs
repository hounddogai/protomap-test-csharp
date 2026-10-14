namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Functions of scheduled visitor manifest from bundle.</summary>
public static class ScheduledVisitorManifestFromBundle
{
    public static Dictionary<string, double> SummarizeStrictFloorPlanBundlesForManifest(List<StrictFloorPlanBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<ScheduledVisitorManifest> DeriveScheduledVisitorManifestsFromBundle(List<StrictFloorPlanBundle> items)
    {
        var outValue = new List<ScheduledVisitorManifest>();
        foreach (var item in items)
        {
            var derived = ScheduledVisitorManifest.NewScheduledVisitorManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveScheduledVisitorManifest(ScheduledVisitorManifest item)
    {
        return FloorPlanProfilePolicy.ClassifyFloorPlanProfile(item.ProjectedVolume());
    }
}
