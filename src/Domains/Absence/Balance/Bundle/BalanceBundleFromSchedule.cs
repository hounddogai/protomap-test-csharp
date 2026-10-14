namespace Acme.Workforce.Absence.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Functions of balance bundle from schedule.</summary>
public static class BalanceBundleFromSchedule
{
    public static Dictionary<string, double> SummarizeGlobalBalanceSchedulesForBundle(List<GlobalBalanceSchedule> items)
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

    public static List<BalanceBundle> DeriveBalanceBundlesFromSchedule(List<GlobalBalanceSchedule> items)
    {
        var outValue = new List<BalanceBundle>();
        foreach (var item in items)
        {
            var derived = BalanceBundle.NewBalanceBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBalanceBundle(BalanceBundle item)
    {
        return BalanceAdjustmentPolicy.ClassifyBalanceAdjustment(item.WeightedExposure());
    }
}
