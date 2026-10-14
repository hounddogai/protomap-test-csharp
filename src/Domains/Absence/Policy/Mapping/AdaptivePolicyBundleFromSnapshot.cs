namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Functions of adaptive policy bundle from snapshot.</summary>
public static class AdaptivePolicyBundleFromSnapshot
{
    public static Dictionary<string, double> SummarizeAccrualSnapshotsForBundle(List<AccrualSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<AdaptivePolicyBundle> DeriveAdaptivePolicyBundlesFromSnapshot(List<AccrualSnapshot> items)
    {
        var outValue = new List<AdaptivePolicyBundle>();
        foreach (var item in items)
        {
            var derived = AdaptivePolicyBundle.NewAdaptivePolicyBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAdaptivePolicyBundle(AdaptivePolicyBundle item)
    {
        return AccrualLedgerPolicy.NormalizeAccrualLedgerCode(item.Code);
    }
}
