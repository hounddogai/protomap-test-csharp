namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Functions of strict occupancy batch from schedule.</summary>
public static class StrictOccupancyBatchFromSchedule
{
    public static Dictionary<string, double> SummarizeOccupancySchedulesForBatch(List<OccupancySchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<StrictOccupancyBatch> DeriveStrictOccupancyBatchesFromSchedule(List<OccupancySchedule> items)
    {
        var outValue = new List<StrictOccupancyBatch>();
        foreach (var item in items)
        {
            var derived = StrictOccupancyBatch.NewStrictOccupancyBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignStrictOccupancyBatch(StrictOccupancyBatch item)
    {
        return OccupancyAdjustmentPolicy.NormalizeOccupancyAdjustmentCode(item.Code);
    }
}
