namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Functions of seasonal visitor schedule from adjustment.</summary>
public static class SeasonalVisitorScheduleFromAdjustment
{
    public static Dictionary<string, double> SummarizeFallbackOccupancyAdjustmentsForSchedule(List<FallbackOccupancyAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<SeasonalVisitorSchedule> DeriveSeasonalVisitorSchedulesFromAdjustment(List<FallbackOccupancyAdjustment> items)
    {
        var outValue = new List<SeasonalVisitorSchedule>();
        foreach (var item in items)
        {
            var derived = SeasonalVisitorSchedule.NewSeasonalVisitorSchedule(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSeasonalVisitorSchedule(SeasonalVisitorSchedule item)
    {
        return FallbackOccupancyAdjustmentPolicy.ClassifyFallbackOccupancyAdjustment(item.ProjectedLoad());
    }
}
