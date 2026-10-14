namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

/// <summary>Functions of pending pay band plan from allocation.</summary>
public static class PendingPayBandPlanFromAllocation
{
    public static Dictionary<string, double> SummarizeScheduledAdjustmentAllocationsForPlan(List<ScheduledAdjustmentAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PendingPayBandPlan> DerivePendingPayBandPlansFromAllocation(List<ScheduledAdjustmentAllocation> items)
    {
        var outValue = new List<PendingPayBandPlan>();
        foreach (var item in items)
        {
            var derived = PendingPayBandPlan.NewPendingPayBandPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocatePendingPayBandPlan(PendingPayBandPlan item)
    {
        return ScheduledAdjustmentAllocationPolicy.ScheduledAdjustmentAllocationBucket(item.EffectiveLoad());
    }
}
