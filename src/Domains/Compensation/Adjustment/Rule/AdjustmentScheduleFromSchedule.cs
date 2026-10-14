namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Functions of adjustment schedule from schedule.</summary>
public static class AdjustmentScheduleFromSchedule
{
    public static Dictionary<string, double> SummarizeBonusPoolSchedulesForSchedule(List<BonusPoolSchedule> items)
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

    public static List<AdjustmentSchedule> DeriveAdjustmentSchedulesFromSchedule(List<BonusPoolSchedule> items)
    {
        var outValue = new List<AdjustmentSchedule>();
        foreach (var item in items)
        {
            var derived = AdjustmentSchedule.NewAdjustmentSchedule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAdjustmentSchedule(AdjustmentSchedule item)
    {
        return BonusPoolSchedulePolicy.ClassifyBonusPoolSchedule(item.AdjustedTotal());
    }
}
