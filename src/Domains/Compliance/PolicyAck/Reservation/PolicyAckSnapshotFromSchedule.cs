namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Functions of policy ack snapshot from schedule.</summary>
public static class PolicyAckSnapshotFromSchedule
{
    public static Dictionary<string, double> SummarizeAttestationSchedulesForSnapshot(List<AttestationSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PolicyAckSnapshot> DerivePolicyAckSnapshotsFromSchedule(List<AttestationSchedule> items)
    {
        var outValue = new List<PolicyAckSnapshot>();
        foreach (var item in items)
        {
            var derived = PolicyAckSnapshot.NewPolicyAckSnapshot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimatePolicyAckSnapshot(PolicyAckSnapshot item)
    {
        return AttestationSchedulePolicy.AttestationScheduleWeight(9, true);
    }
}
