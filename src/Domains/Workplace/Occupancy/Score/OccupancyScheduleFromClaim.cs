namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Functions of occupancy schedule from claim.</summary>
public static class OccupancyScheduleFromClaim
{
    public static Dictionary<string, double> SummarizeRegionalDeskClaimsForSchedule(List<RegionalDeskClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<OccupancySchedule> DeriveOccupancySchedulesFromClaim(List<RegionalDeskClaim> items)
    {
        var outValue = new List<OccupancySchedule>();
        foreach (var item in items)
        {
            var derived = OccupancySchedule.NewOccupancySchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeOccupancySchedule(OccupancySchedule item)
    {
        return RegionalDeskClaimPolicy.RegionalDeskClaimBucket(item.NetExposure());
    }
}
