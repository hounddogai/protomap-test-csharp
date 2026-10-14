namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Functions of fallback policy ack snapshot from proposal.</summary>
public static class FallbackPolicyAckSnapshotFromProposal
{
    public static Dictionary<string, double> SummarizeTrainingDueProposalsForSnapshot(List<TrainingDueProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<FallbackPolicyAckSnapshot> DeriveFallbackPolicyAckSnapshotsFromProposal(List<TrainingDueProposal> items)
    {
        var outValue = new List<FallbackPolicyAckSnapshot>();
        foreach (var item in items)
        {
            var derived = FallbackPolicyAckSnapshot.NewFallbackPolicyAckSnapshot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleFallbackPolicyAckSnapshot(FallbackPolicyAckSnapshot item)
    {
        return PendingTrainingDueExceptionPolicy.ClassifyPendingTrainingDueException(item.ProjectedValue());
    }
}
