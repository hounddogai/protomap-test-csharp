namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Functions of seasonal visitor schedule from checkpoint.</summary>
public static class SeasonalVisitorScheduleFromCheckpoint
{
    public static Dictionary<string, double> SummarizeSeasonalFloorPlanCheckpointsForSchedule(List<SeasonalFloorPlanCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<SeasonalVisitorSchedule> DeriveSeasonalVisitorSchedulesFromCheckpoint(List<SeasonalFloorPlanCheckpoint> items)
    {
        var outValue = new List<SeasonalVisitorSchedule>();
        foreach (var item in items)
        {
            var derived = SeasonalVisitorSchedule.NewSeasonalVisitorSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileSeasonalVisitorSchedule(SeasonalVisitorSchedule item)
    {
        return GlobalFloorPlanRollupPolicy.GlobalFloorPlanRollupWeight(1, true);
    }
}
