namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Functions of leave request budget from ledger.</summary>
public static class LeaveRequestBudgetFromLedger
{
    public static Dictionary<string, double> SummarizeBalanceLedgersForBudget(List<BalanceLedger> items)
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

    public static List<LeaveRequestBudget> DeriveLeaveRequestBudgetsFromLedger(List<BalanceLedger> items)
    {
        var outValue = new List<LeaveRequestBudget>();
        foreach (var item in items)
        {
            var derived = LeaveRequestBudget.NewLeaveRequestBudget(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeLeaveRequestBudget(LeaveRequestBudget item)
    {
        return BalanceAuditPolicy.BalanceAuditBucket(item.MeasuredValue());
    }
}
