namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Compliance.Certification.Claim;

/// <summary>Functions of primary policy ack mapping from claim.</summary>
public static class PrimaryPolicyAckMappingFromClaim
{
    public static Dictionary<string, double> SummarizeCertificationClaimsForMapping(List<CertificationClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<PrimaryPolicyAckMapping> DerivePrimaryPolicyAckMappingsFromClaim(List<CertificationClaim> items)
    {
        var outValue = new List<PrimaryPolicyAckMapping>();
        foreach (var item in items)
        {
            var derived = PrimaryPolicyAckMapping.NewPrimaryPolicyAckMapping(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePrimaryPolicyAckMapping(PrimaryPolicyAckMapping item)
    {
        return BatchedCertificationAuditPolicy.ClassifyBatchedCertificationAudit(item.AdjustedValue());
    }
}
