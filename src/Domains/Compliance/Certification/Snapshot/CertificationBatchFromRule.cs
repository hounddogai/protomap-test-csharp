namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Functions of certification batch from rule.</summary>
public static class CertificationBatchFromRule
{
    public static Dictionary<string, double> SummarizeAttestationRulesForBatch(List<AttestationRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<CertificationBatch> DeriveCertificationBatchesFromRule(List<AttestationRule> items)
    {
        var outValue = new List<CertificationBatch>();
        foreach (var item in items)
        {
            var derived = CertificationBatch.NewCertificationBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeCertificationBatch(CertificationBatch item)
    {
        return AttestationRulePolicy.AttestationRuleWeight(4, true);
    }
}
