namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Functions of attestation override from allocation.</summary>
public static class AttestationOverrideFromAllocation
{
    public static Dictionary<string, double> SummarizeCappedExceptionAllocationsForOverride(List<CappedExceptionAllocation> items)
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

    public static List<AttestationOverride> DeriveAttestationOverridesFromAllocation(List<CappedExceptionAllocation> items)
    {
        var outValue = new List<AttestationOverride>();
        foreach (var item in items)
        {
            var derived = AttestationOverride.NewAttestationOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAttestationOverride(AttestationOverride item)
    {
        return ExceptionBatchPolicy.NormalizeExceptionBatchCode(item.Code);
    }
}
