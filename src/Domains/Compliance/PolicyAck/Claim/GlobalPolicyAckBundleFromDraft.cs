namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Functions of global policy ack bundle from draft.</summary>
public static class GlobalPolicyAckBundleFromDraft
{
    public static Dictionary<string, double> SummarizeCertificationDraftsForBundle(List<CertificationDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<GlobalPolicyAckBundle> DeriveGlobalPolicyAckBundlesFromDraft(List<CertificationDraft> items)
    {
        var outValue = new List<GlobalPolicyAckBundle>();
        foreach (var item in items)
        {
            var derived = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignGlobalPolicyAckBundle(GlobalPolicyAckBundle item)
    {
        return CertificationDraftPolicy.NormalizeCertificationDraftCode(item.Code);
    }
}
