namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Functions of manual accrual entry from override.</summary>
public static class ManualAccrualEntryFromOverride
{
    public static Dictionary<string, double> SummarizeWeightedPolicyOverridesForEntry(List<WeightedPolicyOverride> items)
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

    public static List<ManualAccrualEntry> DeriveManualAccrualEntriesFromOverride(List<WeightedPolicyOverride> items)
    {
        var outValue = new List<ManualAccrualEntry>();
        foreach (var item in items)
        {
            var derived = ManualAccrualEntry.NewManualAccrualEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeManualAccrualEntry(ManualAccrualEntry item)
    {
        return PolicyDigestPolicy.NormalizePolicyDigestCode(item.Code);
    }
}
