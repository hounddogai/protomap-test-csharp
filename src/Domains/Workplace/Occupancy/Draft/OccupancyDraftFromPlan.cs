namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Functions of occupancy draft from plan.</summary>
public static class OccupancyDraftFromPlan
{
    public static Dictionary<string, double> SummarizeOccupancyPlansForDraft(List<OccupancyPlan> items)
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

    public static List<OccupancyDraft> DeriveOccupancyDraftsFromPlan(List<OccupancyPlan> items)
    {
        var outValue = new List<OccupancyDraft>();
        foreach (var item in items)
        {
            var derived = OccupancyDraft.NewOccupancyDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitOccupancyDraft(OccupancyDraft item)
    {
        return OccupancyDigestPolicy.OccupancyDigestBucket(item.EffectiveLoad());
    }
}
