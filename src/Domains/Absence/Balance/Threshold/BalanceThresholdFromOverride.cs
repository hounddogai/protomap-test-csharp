namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Functions of balance threshold from override.</summary>
public static class BalanceThresholdFromOverride
{
    public static Dictionary<string, double> SummarizePrimaryPolicyOverridesForThreshold(List<PrimaryPolicyOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<BalanceThreshold> DeriveBalanceThresholdsFromOverride(List<PrimaryPolicyOverride> items)
    {
        var outValue = new List<BalanceThreshold>();
        foreach (var item in items)
        {
            var derived = BalanceThreshold.NewBalanceThreshold(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveBalanceThreshold(BalanceThreshold item)
    {
        return RollingPolicyOverridePolicy.RollingPolicyOverrideWeight(2, true);
    }
}
