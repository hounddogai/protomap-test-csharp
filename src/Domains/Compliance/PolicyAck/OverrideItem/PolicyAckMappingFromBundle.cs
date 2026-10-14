namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Functions of policy ack mapping from bundle.</summary>
public static class PolicyAckMappingFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalAttestationBundlesForMapping(List<GlobalAttestationBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<PolicyAckMapping> DerivePolicyAckMappingsFromBundle(List<GlobalAttestationBundle> items)
    {
        var outValue = new List<PolicyAckMapping>();
        foreach (var item in items)
        {
            var derived = PolicyAckMapping.NewPolicyAckMapping(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyPolicyAckMappingExtra(PolicyAckMapping item)
    {
        return AttestationAgreementPolicy.AttestationAgreementWeight(6, true);
    }
}
