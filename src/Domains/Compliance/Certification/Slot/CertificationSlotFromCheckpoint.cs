namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Functions of certification slot from checkpoint.</summary>
public static class CertificationSlotFromCheckpoint
{
    public static Dictionary<string, double> SummarizePolicyAckCheckpointsForSlot(List<PolicyAckCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<CertificationSlot> DeriveCertificationSlotsFromCheckpoint(List<PolicyAckCheckpoint> items)
    {
        var outValue = new List<CertificationSlot>();
        foreach (var item in items)
        {
            var derived = CertificationSlot.NewCertificationSlot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeCertificationSlot(CertificationSlot item)
    {
        return PolicyAckCheckpointPolicy.PolicyAckCheckpointBucket(item.ProjectedLoad());
    }
}
