namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Functions of attestation threshold from ledger.</summary>
public static class AttestationThresholdFromLedger
{
    public static Dictionary<string, double> SummarizeBatchedPolicyAckLedgersForThreshold(List<BatchedPolicyAckLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<AttestationThreshold> DeriveAttestationThresholdsFromLedger(List<BatchedPolicyAckLedger> items)
    {
        var outValue = new List<AttestationThreshold>();
        foreach (var item in items)
        {
            var derived = AttestationThreshold.NewAttestationThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignAttestationThreshold(AttestationThreshold item)
    {
        return TieredPolicyAckVariancePolicy.NormalizeTieredPolicyAckVarianceCode(item.Code);
    }
}
