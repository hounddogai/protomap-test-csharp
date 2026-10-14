namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Functions of adaptive balance baseline from segment.</summary>
public static class AdaptiveBalanceBaselineFromSegment
{
    public static Dictionary<string, double> SummarizeLeaveRequestSegmentsForBaseline(List<LeaveRequestSegment> items)
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

    public static List<AdaptiveBalanceBaseline> DeriveAdaptiveBalanceBaselinesFromSegment(List<LeaveRequestSegment> items)
    {
        var outValue = new List<AdaptiveBalanceBaseline>();
        foreach (var item in items)
        {
            var derived = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleAdaptiveBalanceBaseline(AdaptiveBalanceBaseline item)
    {
        return TieredLeaveRequestPlanPolicy.TieredLeaveRequestPlanWeight(7, true);
    }
}
