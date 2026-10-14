namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.Certification.Snapshot;

/// <summary>Functions of policy ack preference from snapshot.</summary>
public static class PolicyAckPreferenceFromSnapshot
{
    public static Dictionary<string, double> SummarizeTieredCertificationSnapshotsForPreference(List<TieredCertificationSnapshot> items)
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

    public static List<PolicyAckPreference> DerivePolicyAckPreferencesFromSnapshot(List<TieredCertificationSnapshot> items)
    {
        var outValue = new List<PolicyAckPreference>();
        foreach (var item in items)
        {
            var derived = PolicyAckPreference.NewPolicyAckPreference(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectPolicyAckPreference(PolicyAckPreference item)
    {
        return TieredCertificationSnapshotPolicy.TieredCertificationSnapshotWeight(9, true);
    }
}
