namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Functions of leave request quota from rollup.</summary>
public static class LeaveRequestQuotaFromRollup
{
    public static Dictionary<string, double> SummarizeAccrualRollupsForQuota(List<AccrualRollup> items)
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

    public static List<LeaveRequestQuota> DeriveLeaveRequestQuotasFromRollup(List<AccrualRollup> items)
    {
        var outValue = new List<LeaveRequestQuota>();
        foreach (var item in items)
        {
            var derived = LeaveRequestQuota.NewLeaveRequestQuota(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLeaveRequestQuota(LeaveRequestQuota item)
    {
        return AccrualRollupPolicy.NormalizeAccrualRollupCode(item.Code);
    }
}
