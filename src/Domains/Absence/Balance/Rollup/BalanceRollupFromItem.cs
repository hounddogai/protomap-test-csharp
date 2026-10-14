namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Functions of balance rollup from item.</summary>
public static class BalanceRollupFromItem
{
    public static Dictionary<string, double> SummarizeDefaultLeaveRequestItemsForRollup(List<DefaultLeaveRequestItem> items)
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

    public static List<BalanceRollup> DeriveBalanceRollupsFromItem(List<DefaultLeaveRequestItem> items)
    {
        var outValue = new List<BalanceRollup>();
        foreach (var item in items)
        {
            var derived = BalanceRollup.NewBalanceRollup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveBalanceRollup(BalanceRollup item)
    {
        return DefaultLeaveRequestItemPolicy.NormalizeDefaultLeaveRequestItemCode(item.Code);
    }
}
