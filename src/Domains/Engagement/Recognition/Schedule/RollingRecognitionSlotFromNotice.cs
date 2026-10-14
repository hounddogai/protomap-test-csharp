namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Functions of rolling recognition slot from notice.</summary>
public static class RollingRecognitionSlotFromNotice
{
    public static Dictionary<string, double> SummarizeActionPlanNoticesForSlot(List<ActionPlanNotice> items)
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

    public static List<RollingRecognitionSlot> DeriveRollingRecognitionSlotsFromNotice(List<ActionPlanNotice> items)
    {
        var outValue = new List<RollingRecognitionSlot>();
        foreach (var item in items)
        {
            var derived = RollingRecognitionSlot.NewRollingRecognitionSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchRollingRecognitionSlot(RollingRecognitionSlot item)
    {
        return TieredActionPlanReservationPolicy.TieredActionPlanReservationBucket(item.AdjustedTotal());
    }
}
