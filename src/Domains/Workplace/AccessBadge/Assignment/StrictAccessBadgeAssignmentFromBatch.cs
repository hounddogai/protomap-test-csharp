namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Functions of strict access badge assignment from batch.</summary>
public static class StrictAccessBadgeAssignmentFromBatch
{
    public static Dictionary<string, double> SummarizePrimaryVisitorBatchesForAssignment(List<PrimaryVisitorBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<StrictAccessBadgeAssignment> DeriveStrictAccessBadgeAssignmentsFromBatch(List<PrimaryVisitorBatch> items)
    {
        var outValue = new List<StrictAccessBadgeAssignment>();
        foreach (var item in items)
        {
            var derived = StrictAccessBadgeAssignment.NewStrictAccessBadgeAssignment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStrictAccessBadgeAssignment(StrictAccessBadgeAssignment item)
    {
        return FallbackVisitorCyclePolicy.FallbackVisitorCycleBucket(item.WeightedValue());
    }
}
