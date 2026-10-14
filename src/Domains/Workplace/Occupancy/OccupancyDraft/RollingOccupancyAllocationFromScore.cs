namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Functions of rolling occupancy allocation from score.</summary>
public static class RollingOccupancyAllocationFromScore
{
    public static Dictionary<string, double> SummarizeAccessBadgeScoresForAllocation(List<AccessBadgeScore> items)
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

    public static List<RollingOccupancyAllocation> DeriveRollingOccupancyAllocationsFromScore(List<AccessBadgeScore> items)
    {
        var outValue = new List<RollingOccupancyAllocation>();
        foreach (var item in items)
        {
            var derived = RollingOccupancyAllocation.NewRollingOccupancyAllocation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankRollingOccupancyAllocation(RollingOccupancyAllocation item)
    {
        return AccessBadgeScorePolicy.ClassifyAccessBadgeScore(item.AdjustedTotal());
    }
}
