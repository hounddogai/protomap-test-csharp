namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Functions of pending equity grant plan from schedule.</summary>
public static class PendingEquityGrantPlanFromSchedule
{
    public static Dictionary<string, double> SummarizePayBandSchedulesForPlan(List<PayBandSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<PendingEquityGrantPlan> DerivePendingEquityGrantPlansFromSchedule(List<PayBandSchedule> items)
    {
        var outValue = new List<PendingEquityGrantPlan>();
        foreach (var item in items)
        {
            var derived = PendingEquityGrantPlan.NewPendingEquityGrantPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePendingEquityGrantPlan(PendingEquityGrantPlan item)
    {
        return TieredPayBandSchedulePolicy.ClassifyTieredPayBandSchedule(item.NetExposure());
    }
}
