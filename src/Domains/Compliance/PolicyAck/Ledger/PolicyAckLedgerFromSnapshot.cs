namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Functions of policy ack ledger from snapshot.</summary>
public static class PolicyAckLedgerFromSnapshot
{
    public static Dictionary<string, double> SummarizePolicyAckSnapshotsForLedger(List<PolicyAckSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PolicyAckLedger> DerivePolicyAckLedgersFromSnapshot(List<PolicyAckSnapshot> items)
    {
        var outValue = new List<PolicyAckLedger>();
        foreach (var item in items)
        {
            var derived = PolicyAckLedger.NewPolicyAckLedger(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePolicyAckLedger(PolicyAckLedger item)
    {
        return PolicyAckSnapshotPolicy.ClassifyPolicyAckSnapshot(item.ProjectedVolume());
    }
}
