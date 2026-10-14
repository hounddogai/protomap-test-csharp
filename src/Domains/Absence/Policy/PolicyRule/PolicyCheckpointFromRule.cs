namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Functions of policy checkpoint from rule.</summary>
public static class PolicyCheckpointFromRule
{
    public static Dictionary<string, double> SummarizeFallbackPolicyRulesForCheckpoint(List<FallbackPolicyRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<PolicyCheckpoint> DerivePolicyCheckpointsFromRule(List<FallbackPolicyRule> items)
    {
        var outValue = new List<PolicyCheckpoint>();
        foreach (var item in items)
        {
            var derived = PolicyCheckpoint.NewPolicyCheckpoint(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePolicyCheckpoint(PolicyCheckpoint item)
    {
        return BatchedPolicyThresholdPolicy.BatchedPolicyThresholdBucket(item.ProjectedVolume());
    }
}
