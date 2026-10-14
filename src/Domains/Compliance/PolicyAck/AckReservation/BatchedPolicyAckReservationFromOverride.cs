namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Functions of batched policy ack reservation from override.</summary>
public static class BatchedPolicyAckReservationFromOverride
{
    public static Dictionary<string, double> SummarizeAttestationOverridesForReservation(List<AttestationOverride> items)
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

    public static List<BatchedPolicyAckReservation> DeriveBatchedPolicyAckReservationsFromOverride(List<AttestationOverride> items)
    {
        var outValue = new List<BatchedPolicyAckReservation>();
        foreach (var item in items)
        {
            var derived = BatchedPolicyAckReservation.NewBatchedPolicyAckReservation(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyBatchedPolicyAckReservation(BatchedPolicyAckReservation item)
    {
        return AttestationPreferencePolicy.AttestationPreferenceWeight(3, true);
    }
}
