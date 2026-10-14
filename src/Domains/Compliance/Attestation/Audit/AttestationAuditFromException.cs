namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Functions of attestation audit from exception.</summary>
public static class AttestationAuditFromException
{
    public static Dictionary<string, double> SummarizePolicyAckExceptionsForAudit(List<PolicyAckException> items)
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

    public static List<AttestationAudit> DeriveAttestationAuditsFromException(List<PolicyAckException> items)
    {
        var outValue = new List<AttestationAudit>();
        foreach (var item in items)
        {
            var derived = AttestationAudit.NewAttestationAudit(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAttestationAudit(AttestationAudit item)
    {
        return PolicyAckAuditPolicy.PolicyAckAuditBucket(item.AdjustedExposure());
    }
}
