namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Functions of global accrual estimate from adjustment.</summary>
public static class GlobalAccrualEstimateFromAdjustment
{
    public static Dictionary<string, double> SummarizeWeightedLeaveRequestAdjustmentsForEstimate(List<WeightedLeaveRequestAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<GlobalAccrualEstimate> DeriveGlobalAccrualEstimatesFromAdjustment(List<WeightedLeaveRequestAdjustment> items)
    {
        var outValue = new List<GlobalAccrualEstimate>();
        foreach (var item in items)
        {
            var derived = GlobalAccrualEstimate.NewGlobalAccrualEstimate(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeGlobalAccrualEstimate(GlobalAccrualEstimate item)
    {
        return ManualLeaveRequestScorePolicy.NormalizeManualLeaveRequestScoreCode(item.Code);
    }
}
