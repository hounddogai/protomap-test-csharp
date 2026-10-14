namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Functions of strict occupancy segment from schedule.</summary>
public static class StrictOccupancySegmentFromSchedule
{
    public static Dictionary<string, double> SummarizeFloorPlanSchedulesForSegment(List<FloorPlanSchedule> items)
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

    public static List<StrictOccupancySegment> DeriveStrictOccupancySegmentsFromSchedule(List<FloorPlanSchedule> items)
    {
        var outValue = new List<StrictOccupancySegment>();
        foreach (var item in items)
        {
            var derived = StrictOccupancySegment.NewStrictOccupancySegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchStrictOccupancySegment(StrictOccupancySegment item)
    {
        return StrictFloorPlanBundlePolicy.NormalizeStrictFloorPlanBundleCode(item.Code);
    }
}
