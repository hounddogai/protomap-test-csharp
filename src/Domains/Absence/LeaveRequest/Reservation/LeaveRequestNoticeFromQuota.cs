namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Functions of leave request notice from quota.</summary>
public static class LeaveRequestNoticeFromQuota
{
    public static Dictionary<string, double> SummarizeCappedBalanceQuotasForNotice(List<CappedBalanceQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LeaveRequestNotice> DeriveLeaveRequestNoticesFromQuota(List<CappedBalanceQuota> items)
    {
        var outValue = new List<LeaveRequestNotice>();
        foreach (var item in items)
        {
            var derived = LeaveRequestNotice.NewLeaveRequestNotice(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceLeaveRequestNotice(LeaveRequestNotice item)
    {
        return ScheduledBalanceGroupPolicy.NormalizeScheduledBalanceGroupCode(item.Code);
    }
}
