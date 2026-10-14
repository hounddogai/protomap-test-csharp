namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Functions of policy ack estimate from manifest.</summary>
public static class PolicyAckEstimateFromManifest
{
    public static Dictionary<string, double> SummarizeAttestationManifestsForEstimate(List<AttestationManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<PolicyAckEstimate> DerivePolicyAckEstimatesFromManifest(List<AttestationManifest> items)
    {
        var outValue = new List<PolicyAckEstimate>();
        foreach (var item in items)
        {
            var derived = PolicyAckEstimate.NewPolicyAckEstimate(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePolicyAckEstimate(PolicyAckEstimate item)
    {
        return AttestationMilestonePolicy.ClassifyAttestationMilestone(item.EffectiveExposure());
    }
}
