namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Functions of global policy ack cycle from snapshot.</summary>
public static class GlobalPolicyAckCycleFromSnapshot
{
    public static Dictionary<string, double> SummarizeCertificationSnapshotsForCycle(List<CertificationSnapshot> items)
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

    public static List<GlobalPolicyAckCycle> DeriveGlobalPolicyAckCyclesFromSnapshot(List<CertificationSnapshot> items)
    {
        var outValue = new List<GlobalPolicyAckCycle>();
        foreach (var item in items)
        {
            var derived = GlobalPolicyAckCycle.NewGlobalPolicyAckCycle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectGlobalPolicyAckCycle(GlobalPolicyAckCycle item)
    {
        return LenientCertificationAssignmentPolicy.NormalizeLenientCertificationAssignmentCode(item.Code);
    }
}
