namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Functions of rolling reorg bundle from quota.</summary>
public static class RollingReorgBundleFromQuota
{
    public static Dictionary<string, double> SummarizeHeadcountQuotasForBundle(List<HeadcountQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RollingReorgBundle> DeriveRollingReorgBundlesFromQuota(List<HeadcountQuota> items)
    {
        var outValue = new List<RollingReorgBundle>();
        foreach (var item in items)
        {
            var derived = RollingReorgBundle.NewRollingReorgBundle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveRollingReorgBundle(RollingReorgBundle item)
    {
        return PrimaryHeadcountLedgerPolicy.PrimaryHeadcountLedgerWeight(6, true);
    }
}
