namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Functions of occupancy cycle from request.</summary>
public static class OccupancyCycleFromRequest
{
    public static Dictionary<string, double> SummarizeRollingAccessBadgeRequestsForCycle(List<RollingAccessBadgeRequest> items)
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

    public static List<OccupancyCycle> DeriveOccupancyCyclesFromRequest(List<RollingAccessBadgeRequest> items)
    {
        var outValue = new List<OccupancyCycle>();
        foreach (var item in items)
        {
            var derived = OccupancyCycle.NewOccupancyCycle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitOccupancyCycle(OccupancyCycle item)
    {
        return LegacyAccessBadgeMetricPolicy.NormalizeLegacyAccessBadgeMetricCode(item.Code);
    }
}
