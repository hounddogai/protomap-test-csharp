namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Assets.Refresh.Budget;

/// <summary>Functions of pending laptop ledger from budget.</summary>
public static class PendingLaptopLedgerFromBudget
{
    public static Dictionary<string, double> SummarizeRefreshBudgetsForLedger(List<RefreshBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<PendingLaptopLedger> DerivePendingLaptopLedgersFromBudget(List<RefreshBudget> items)
    {
        var outValue = new List<PendingLaptopLedger>();
        foreach (var item in items)
        {
            var derived = PendingLaptopLedger.NewPendingLaptopLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizePendingLaptopLedger(PendingLaptopLedger item)
    {
        return TieredRefreshScorePolicy.TieredRefreshScoreBucket(item.NetVolume());
    }
}
