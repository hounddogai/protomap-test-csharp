namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Functions of leave request bundle from request.</summary>
public static class LeaveRequestBundleFromRequest
{
    public static Dictionary<string, double> SummarizeCappedBalanceRequestsForBundle(List<CappedBalanceRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<LeaveRequestBundle> DeriveLeaveRequestBundlesFromRequest(List<CappedBalanceRequest> items)
    {
        var outValue = new List<LeaveRequestBundle>();
        foreach (var item in items)
        {
            var derived = LeaveRequestBundle.NewLeaveRequestBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLeaveRequestBundle(LeaveRequestBundle item)
    {
        return BalanceRequestPolicy.NormalizeBalanceRequestCode(item.Code);
    }
}
