namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Functions of rolling leave request batch from summary.</summary>
public static class RollingLeaveRequestBatchFromSummary
{
    public static Dictionary<string, double> SummarizeRegionalAccrualSummariesForBatch(List<RegionalAccrualSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<RollingLeaveRequestBatch> DeriveRollingLeaveRequestBatchesFromSummary(List<RegionalAccrualSummary> items)
    {
        var outValue = new List<RollingLeaveRequestBatch>();
        foreach (var item in items)
        {
            var derived = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateRollingLeaveRequestBatch(RollingLeaveRequestBatch item)
    {
        return RegionalAccrualSummaryPolicy.NormalizeRegionalAccrualSummaryCode(item.Code);
    }
}
