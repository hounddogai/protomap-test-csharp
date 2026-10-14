namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Functions of tiered exception group from threshold.</summary>
public static class TieredExceptionGroupFromThreshold
{
    public static Dictionary<string, double> SummarizeAttestationThresholdsForGroup(List<AttestationThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<TieredExceptionGroup> DeriveTieredExceptionGroupsFromThreshold(List<AttestationThreshold> items)
    {
        var outValue = new List<TieredExceptionGroup>();
        foreach (var item in items)
        {
            var derived = TieredExceptionGroup.NewTieredExceptionGroup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTieredExceptionGroup(TieredExceptionGroup item)
    {
        return GlobalAttestationGroupPolicy.NormalizeGlobalAttestationGroupCode(item.Code);
    }
}
