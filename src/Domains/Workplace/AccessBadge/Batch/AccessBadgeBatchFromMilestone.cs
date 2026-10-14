namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Functions of access badge batch from milestone.</summary>
public static class AccessBadgeBatchFromMilestone
{
    public static Dictionary<string, double> SummarizeDeskMilestonesForBatch(List<DeskMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<AccessBadgeBatch> DeriveAccessBadgeBatchesFromMilestone(List<DeskMilestone> items)
    {
        var outValue = new List<AccessBadgeBatch>();
        foreach (var item in items)
        {
            var derived = AccessBadgeBatch.NewAccessBadgeBatch(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveAccessBadgeBatch(AccessBadgeBatch item)
    {
        return DeskCheckpointPolicy.DeskCheckpointWeight(6, true);
    }
}
