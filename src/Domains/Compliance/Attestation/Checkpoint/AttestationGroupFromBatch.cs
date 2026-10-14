namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Certification.Snapshot;

/// <summary>Functions of attestation group from batch.</summary>
public static class AttestationGroupFromBatch
{
    public static Dictionary<string, double> SummarizeCertificationBatchesForGroup(List<CertificationBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<AttestationGroup> DeriveAttestationGroupsFromBatch(List<CertificationBatch> items)
    {
        var outValue = new List<AttestationGroup>();
        foreach (var item in items)
        {
            var derived = AttestationGroup.NewAttestationGroup(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileAttestationGroup(AttestationGroup item)
    {
        return TieredCertificationSnapshotPolicy.TieredCertificationSnapshotWeight(9, true);
    }
}
