namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Absence.Balance.Bundle;

/// <summary>Functions of accrual target from bundle.</summary>
public static class AccrualTargetFromBundle
{
    public static Dictionary<string, double> SummarizeBalanceBundlesForTarget(List<BalanceBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<AccrualTarget> DeriveAccrualTargetsFromBundle(List<BalanceBundle> items)
    {
        var outValue = new List<AccrualTarget>();
        foreach (var item in items)
        {
            var derived = AccrualTarget.NewAccrualTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceAccrualTarget(AccrualTarget item)
    {
        return BatchedBalanceOverridePolicy.ClassifyBatchedBalanceOverride(item.WeightedExposure());
    }
}
