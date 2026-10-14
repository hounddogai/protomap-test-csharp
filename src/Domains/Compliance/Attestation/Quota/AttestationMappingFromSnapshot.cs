namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Functions of attestation mapping from snapshot.</summary>
public static class AttestationMappingFromSnapshot
{
    public static Dictionary<string, double> SummarizeGlobalExceptionSnapshotsForMapping(List<GlobalExceptionSnapshot> items)
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

    public static List<AttestationMapping> DeriveAttestationMappingsFromSnapshot(List<GlobalExceptionSnapshot> items)
    {
        var outValue = new List<AttestationMapping>();
        foreach (var item in items)
        {
            var derived = AttestationMapping.NewAttestationMapping(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitAttestationMapping(AttestationMapping item)
    {
        return ExceptionQuotaPolicy.ExceptionQuotaWeight(9, true);
    }
}
